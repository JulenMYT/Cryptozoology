using UnityEngine;
using UnityEngine.AI;
using System;

public class AnimalLeave : MonoBehaviour, IAnimalBehaviour
{
    public event Action FinishedLeaving;

    [SerializeField] private NavMeshAgent agent;

    private Transform spawnPoint;
    private bool active = false;

    public bool IsActive() => active;

    public void SetSpawnPoint(Transform point)
    {
        spawnPoint = point;
    }

    public void Activate()
    {
        if (spawnPoint == null) return;
        active = true;
        agent.SetDestination(spawnPoint.position);
    }

    public void Deactivate()
    {
        active = false;
        agent.ResetPath();
    }

    private void Update()
    {
        if (!active) return;
        if (spawnPoint == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            FinishedLeaving?.Invoke();
        }
    }
}
