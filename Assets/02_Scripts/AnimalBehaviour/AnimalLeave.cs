using UnityEngine;
using UnityEngine.AI;
using System;

public class AnimalLeave : MonoBehaviour, IAnimalBehaviour
{
    public event Action FinishedLeaving;

    [SerializeField] private NavMeshAgent agent;

    [SerializeField]
    private Vector3 leavePoint;
    private bool active = false;

    public bool IsActive() => active;

    public void SetLeavePoint(Vector3 point)
    {
        leavePoint = point;
    }

    public void Activate()
    {
        if (leavePoint == null)
        {
            SetLeavePoint(GameManager.Instance.AnimalSpawner.GetRandomLeavePoint());
        }

        active = true;
        agent.SetDestination(leavePoint);
    }

    public void Deactivate()
    {
        active = false;
        agent.ResetPath();
    }

    private void Update()
    {
        if (!active) return;
        if (leavePoint == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            FinishedLeaving?.Invoke();
        }
    }
}
