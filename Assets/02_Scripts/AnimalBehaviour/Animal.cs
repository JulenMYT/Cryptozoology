using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AnimalState
{
    Idle,
    Wandering,
    Eating,
    Sleeping,
    Patrolling,
    Leaving
}

public enum AnimalType
{
    NonResident,
    Visitor,
    Resident
}

public class Animal : MonoBehaviour, IEdible
{
    [SerializeField] private AnimalController controller;
    [SerializeField] private AnimalWander wander;
    [SerializeField] private AnimalEating eating;
    [SerializeField] private AnimalPatrol patrol;
    [SerializeField] private AnimalLeave leave;
    [SerializeField] private AnimalSleep sleep;
    [SerializeField] private AnimalDataSO data;
    [SerializeField] private float detectionRadius;

    private const float FoodCheckInterval = 0.5f;
    private float foodCheckTimer;
    private NavMeshAgent agent;
    private Dictionary<string, int> eatenCounts = new();

    public AnimalState State { get; private set; } = AnimalState.Idle;
    public AnimalType Type { get; private set; } = AnimalType.NonResident;
    private bool safe;

    public event Action BecameResident;
    public event Action LeftGarden;

    public AnimalSaveData AnimalSaveData = new();
    private bool placed = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority += UnityEngine.Random.Range(-10, 10);

        if (eating != null)
        {
            eating.Eating += OnStartedEating;
            eating.DoneEating += OnDoneEating;
        }

        if (sleep != null)
        {
            sleep.EnterHouse += () => safe = true;
            sleep.ExitHouse += () => safe = false;
        }
    }

    private void Update()
    {
        if (!placed) return;
        if (State == AnimalState.Leaving) return;

        switch (Type)
        {
            case AnimalType.NonResident:
                HandleNonResident();
                break;
            case AnimalType.Visitor:
                HandleVisitor();
                break;
            case AnimalType.Resident:
                HandleResident();
                break;
        }
    }

    private void HandleNonResident()
    {
        if (data.ShouldSleep())
        {
            StartLeaving();
            return;
        }

        if (CanVisitGarden())
        {
            BecomeVisitor();
        }
    }

    private void HandleVisitor()
    {
        if (CanBecomeResident())
        {
            BecomeResident();
            return;
        }

        if (data.ShouldSleep() && State != AnimalState.Eating)
        {
            StartLeaving();
            return;
        }

        TryFindFood();
    }

    private void HandleResident()
    {
        if (data.ShouldSleep() && State != AnimalState.Sleeping)
        {
            var house = GameManager.Instance.Garden.GetObject(data.houseID);
            if (house != null) sleep.SetHouse(house);
            controller.SetBehaviour(sleep);
            State = AnimalState.Sleeping;
        }
        else if (!data.ShouldSleep() && State == AnimalState.Sleeping)
        {
            controller.SetBehaviour(wander);
            State = AnimalState.Wandering;
        }
    }

    private void TryFindFood()
    {
        if (State == AnimalState.Eating) return;

        foodCheckTimer += Time.deltaTime;
        if (foodCheckTimer < FoodCheckInterval) return;

        foodCheckTimer = 0f;
        var target = DetectFood();
        if (target == null) return;
        eating.SetTarget(target);
        controller.SetBehaviour(eating);
        State = AnimalState.Eating;
    }

    private IEdible DetectFood()
    {
        var hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IEdible>(out var edible)) continue;
            if (!edible.CanBeEaten()) continue;
            if (data.conditions.residenceCondition.eatingConditions.Exists(c => c.id == edible.GetId()))
                return edible;
        }
        return null;
    }

    private void OnStartedEating(string id)
    {
        if (!eatenCounts.ContainsKey(id)) eatenCounts[id] = 0;
        eatenCounts[id]++;
    }

    private void OnDoneEating()
    {
        controller.SetBehaviour(wander);
        State = AnimalState.Wandering;
    }

    private void StartLeaving()
    {
        controller.SetBehaviour(leave);
        leave.FinishedLeaving -= LeaveGarden;
        leave.FinishedLeaving += LeaveGarden;
        State = AnimalState.Leaving;
    }

    public void Initialize(AnimalType type = AnimalType.NonResident)
    {
        SaveManager.Instance.OnSave += Save;

        placed = true;
        AnimalSaveData.name = data.displayName;
        AnimalSaveData.ID = SaveData.GenerateID();

        switch (type)
        {
            case AnimalType.NonResident:
                State = AnimalState.Patrolling;
                controller.SetBehaviour(patrol);
                wander.SetZone(NavZone.Outside);
                NavMeshZoneManager.SetAgentZone(agent, NavZone.Outside);
                break;

            case AnimalType.Visitor:
                BecomeVisitor();
                break;

            case AnimalType.Resident:
                BecomeResident();
                break;
        }
    }

    public void Initialize(AnimalSaveData animalSaveData)
    {
        SaveManager.Instance.OnSave += Save;

        placed = true;
        AnimalSaveData = animalSaveData;
        transform.rotation = animalSaveData.Rotation;
        eatenCounts = animalSaveData.eatenCounts;

        switch (animalSaveData.Type)
        {
            case AnimalType.NonResident:
                State = AnimalState.Patrolling;
                controller.SetBehaviour(patrol);
                wander.SetZone(NavZone.Outside);
                NavMeshZoneManager.SetAgentZone(agent, NavZone.Outside);
                break;

            case AnimalType.Visitor:
                BecomeVisitor();
                break;

            case AnimalType.Resident:
                BecomeResident();
                break;
        }
    }

    public void BecomeVisitor()
    {
        Type = AnimalType.Visitor;
        State = AnimalState.Wandering;
        controller.SetBehaviour(wander);
        wander.SetZone(NavZone.Garden);
        NavMeshZoneManager.SetAgentZone(agent, NavZone.All);
    }

    public void BecomeResident()
    {
        if (Type == AnimalType.Resident) return;

        Type = AnimalType.Resident;
        State = AnimalState.Wandering;
        controller.SetBehaviour(wander);
        wander.SetZone(NavZone.Garden);
        GameManager.Instance.Garden.AddObject(data.displayName, gameObject);
        BecameResident?.Invoke();
        UnlockSection(1);
        RegisterAnimal();
    }

    public void LeaveGarden()
    {
        LeftGarden?.Invoke();
        Destroy(gameObject);
    }

    public bool CanBecomeResident()
    {
        foreach (var cond in data.conditions.residenceCondition.eatingConditions)
            if (!eatenCounts.ContainsKey(cond.id) || eatenCounts[cond.id] < cond.minCount) return false;

        foreach (var cond in data.conditions.residenceCondition.placingConditions)
            if (GameManager.Instance.Garden.GetCount(cond.id) < cond.minCount) return false;

        return true;
    }

    public bool CanVisitGarden()
    {
        foreach (var cond in data.conditions.visitCondition.placingConditions)
            if (GameManager.Instance.Garden.GetCount(cond.id) < cond.minCount) return false;

        return true;
    }

    public bool CanBeEaten() => Type == AnimalType.Resident && !safe;

    public void Eat()
    {
        GameManager.Instance.Garden.RemoveObject(data.displayName, gameObject);
        Destroy(gameObject);
    }

    public string GetId() => data.displayName;

    private void UnlockSection(int level)
    {
        GameManager.Instance.Encyclopedia.UnlockSection(data.displayName, level);
    }

    private void RegisterAnimal()
    {
        GameManager.Instance.Animals.RegisterAnimal(data, 1);
    }

    private void OnDestroy()
    {
        GameManager.Instance.Animals.RemoveAnimal(data, 0);
    }

    private void Save()
    {
        AnimalSaveData.Type = Type;
        AnimalSaveData.State = State;
        AnimalSaveData.Position = transform.position;
        AnimalSaveData.Rotation = transform.rotation;
        AnimalSaveData.Scale = transform.localScale;

        if (Type != AnimalType.Resident)
        {
            AnimalSaveData.eatenCounts = eatenCounts;
        }

        SaveManager.Instance.saveData.AddData(AnimalSaveData);
    }
}
