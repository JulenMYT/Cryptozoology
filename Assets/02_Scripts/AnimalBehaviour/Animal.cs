using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour, IEdible
{
    [SerializeField] private AnimalController controller;
    [SerializeField] private AnimalWander wanderBehaviour;
    [SerializeField] private AnimalEating eatingBehaviour;
    [SerializeField] private AnimalPatrol patrolBehaviour;
    [SerializeField] private AnimalLeave leaveBehaviour;
    [SerializeField] private AnimalSleep sleepBehaviour;

    [SerializeField] private float detectionRadius;

    private const float foodCheckInterval = 0.5f;
    private float foodCheckTimer;

    private NavMeshAgent agent;
    private Dictionary<string, int> eatenCounts = new();
    [SerializeField] private AnimalDataSO data;

    private bool isActive;
    private bool visitingGarden;
    private bool safe;

    public bool IsResident { get; private set; }
    public bool IsVisiting => !IsResident;

    private AnimalGroup group;

    public event Action BecameResident;
    public event Action LeftGarden;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.avoidancePriority += UnityEngine.Random.Range(-10, 10);

        if (eatingBehaviour != null)
        {
            eatingBehaviour.Eating += OnStartedEating;
            eatingBehaviour.DoneEating += OnDoneEating;
        }

        if (sleepBehaviour != null)
        {
            sleepBehaviour.EnterHouse += () => safe = true;
            sleepBehaviour.ExitHouse += () => safe = false;
        }
    }

    private void Update()
    {
        if (!isActive || leaveBehaviour.IsActive()) return;

        if (!IsResident)
        {
            if (!visitingGarden) HandlePotentialVisitor();
            else HandleNonResidentBehavior();
        }
        else
        {
            HandleResidentBehavior();
        }
    }

    private void HandlePotentialVisitor()
    {
        if (data.ShouldSleep())
        {
            StartLeave();
            return;
        }

        if (CheckVisitingCondition())
            BecomeVisitor();
    }

    private void HandleNonResidentBehavior()
    {
        if (CheckResidenceCondition())
        {
            BecomeResident();
            return;
        }

        if (data.ShouldSleep() && !eatingBehaviour.IsActive())
        {
            StartLeave();
            return;
        }

        TryEatFood();
    }

    private void HandleResidentBehavior()
    {
        if (data.ShouldSleep() && !sleepBehaviour.IsActive())
        {
            var houseObj = GameManager.Instance.Garden.GetObject(data.houseID);
            if (houseObj != null) sleepBehaviour.SetHouse(houseObj);
            controller.SetBehaviour(sleepBehaviour);
        }
        else if (!data.ShouldSleep() && sleepBehaviour.IsActive())
        {
            controller.SetBehaviour(wanderBehaviour);
        }
    }

    private void TryEatFood()
    {
        if (eatingBehaviour.IsActive()) return;

        foodCheckTimer += Time.deltaTime;
        if (foodCheckTimer < foodCheckInterval) return;

        foodCheckTimer = 0f;
        IEdible target = DetectFood();
        if (target != null)
        {
            eatingBehaviour.SetTarget(target);
            controller.SetBehaviour(eatingBehaviour);
        }
    }

    private IEdible DetectFood()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IEdible>(out var edible) && edible.CanBeEaten())
            {
                if (data.conditions.residenceCondition.eatingConditions.Exists(c => c.id == edible.GetId()))
                    return edible;
            }
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
        controller.SetBehaviour(wanderBehaviour);
    }

    private void StartLeave()
    {
        controller.SetBehaviour(leaveBehaviour);
        leaveBehaviour.FinishedLeaving -= LeaveGarden;
        leaveBehaviour.FinishedLeaving += LeaveGarden;
    }

    public void Initialize()
    {
        IsResident = true;
        isActive = true;
        controller.SetBehaviour(wanderBehaviour);
        wanderBehaviour.SetZone(NavZone.Garden);
        NavMeshZoneManager.SetAgentZone(agent, NavZone.Garden);
        UnlockSection(1);
    }

    public void SpawnAsVisitor()
    {
        IsResident = false;
        visitingGarden = false;
        isActive = true;
        controller.SetBehaviour(patrolBehaviour);
        wanderBehaviour.SetZone(NavZone.Outside);
        NavMeshZoneManager.SetAgentZone(agent, NavZone.Outside);
    }

    public void BecomeVisitor()
    {
        IsResident = false;
        visitingGarden = true;
        isActive = true;
        controller.SetBehaviour(wanderBehaviour);
        wanderBehaviour.SetZone(NavZone.Garden);
        NavMeshZoneManager.SetAgentZone(agent, NavZone.All);
    }

    public void BecomeResident()
    {
        if (IsResident) return;

        IsResident = true;
        controller.SetBehaviour(wanderBehaviour);
        wanderBehaviour.SetZone(NavZone.Garden);
        GameManager.Instance.Garden.AddObject(data.id, gameObject);
        BecameResident?.Invoke();
        UnlockSection(1);
    }

    public void LeaveGarden()
    {
        LeftGarden?.Invoke();
        Destroy(gameObject);
    }

    public bool CheckResidenceCondition()
    {
        foreach (var cond in data.conditions.residenceCondition.eatingConditions)
            if (!eatenCounts.ContainsKey(cond.id) || eatenCounts[cond.id] < cond.minCount) return false;

        foreach (var cond in data.conditions.residenceCondition.placingConditions)
            if (GameManager.Instance.Garden.GetCount(cond.id) < cond.minCount) return false;

        return true;
    }

    public bool CheckVisitingCondition()
    {
        foreach (var cond in data.conditions.visitCondition.placingConditions)
            if (GameManager.Instance.Garden.GetCount(cond.id) < cond.minCount) return false;

        return true;
    }

    public bool CanBeEaten() => IsResident && !safe;

    public void Eat()
    {
        GameManager.Instance.Garden.RemoveObject(data.id, gameObject);
        Destroy(gameObject);
    }

    public string GetId() => data.id;

    private void UnlockSection(int level) => GameManager.Instance.Encyclopedia.UnlockSection(data.id, level);
}
