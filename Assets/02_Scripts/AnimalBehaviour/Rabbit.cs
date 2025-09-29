using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rabbit : MonoBehaviour, IAnimal
{
    [SerializeField] private AnimalController controller;
    [SerializeField] private AnimalWander wanderBehaviour;
    [SerializeField] private AnimalEating eatingBehaviour;
    [SerializeField] private AnimalPatrol patrolBehaviour;
    [SerializeField] private float detectionRadius;
    private NavMeshAgent agent;

    private Dictionary<string, int> eatenCounts = new();
    [SerializeField] private AnimalDataSO data;

    private bool isActive = false;

    public bool IsResident { get; private set; }
    public bool IsVisiting => !IsResident;
    private bool visitingGarden = false;

    private void Awake()
    {
        if (eatingBehaviour != null)
        {
            eatingBehaviour.Eating += OnStartedEating;
            eatingBehaviour.DoneEating += OnDoneEating;
        }
        agent = GetComponent<NavMeshAgent>();

        agent.avoidancePriority += Random.Range(-10, 10);
    }

    private void Update()
    {
        if (!isActive) return;

        if (!visitingGarden)
        {
            if (CheckVisitingCondition())
            {
                BecomeVisitor();
            }
            return;
        }

        if (!IsResident)
        {
            if (CheckResidenceCondition())
            {
                BecomeResident();
                return;
            }

            if (!eatingBehaviour.IsActive())
            {
                IEdible target = DetectFood();
                if (target != null)
                {
                    eatingBehaviour.SetTarget(target);
                    controller.SetBehaviour(eatingBehaviour);
                }
            }

            return;
        }
    }

    private IEdible DetectFood()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IEdible>(out var edible) && edible.CanBeEaten())
            {
                string foodId = edible.GetId();
                if (data.conditions.residenceCondition.eatingConditions.Exists(c => c.id == foodId))
                    return edible;
            }
        }
        return null;
    }

    private void OnStartedEating(string id)
    {
        if (!eatenCounts.ContainsKey(id)) eatenCounts[id] = 0;
        eatenCounts[id]++;
        Debug.Log($"Rabbit started eating {id}");
    }

    private void OnDoneEating()
    {
        controller.SetBehaviour(wanderBehaviour);
    }

    public void Initialize()
    {
        IsResident = true;
        isActive = true;
        controller.SetBehaviour(wanderBehaviour);
        wanderBehaviour.SetZone(NavZone.Garden);
        NavMeshZoneManager.SetAgentZone(agent, NavZone.Garden);
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
        if (IsResident)
            return;
        IsResident = true;
        controller.SetBehaviour(wanderBehaviour);
        wanderBehaviour.SetZone(NavZone.Garden);

        if (TryGetComponent<ObjectIdentity>(out var identity))
            GameManager.Instance.Garden.AddObject(identity.Id, gameObject);
    }

    public bool CheckResidenceCondition()
    {
        foreach (var cond in data.conditions.residenceCondition.eatingConditions)
        {
            if (!eatenCounts.ContainsKey(cond.id) || eatenCounts[cond.id] < cond.minCount)
                return false;
        }

        foreach (var cond in data.conditions.residenceCondition.placingConditions)
        {
            if (GameManager.Instance.Garden.GetCount(cond.id) < cond.minCount)
                return false;
        }

        return true;
    }

    public bool CheckVisitingCondition()
    {
        foreach (var cond in data.conditions.visitCondition.placingConditions)
        {
            if (GameManager.Instance.Garden.GetCount(cond.id) < cond.minCount)
                return false;
        }
        return true;
    }
}
