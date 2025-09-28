using UnityEngine;
using UnityEngine.AI;

public class AnimalWander : MonoBehaviour, IAnimalBehaviour
{
    public float wanderRadius = 5f;
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    [SerializeField]
    private NavMeshAgent agent;
    private float timer;
    private float waitTime;
    private bool active = false;

    private NavZone allowedZone = NavZone.Garden;

    public bool IsActive() => active;

    void Start()
    {
        waitTime = Random.Range(minWaitTime, maxWaitTime);
    }

    void Update()
    {
        if (!active) return;

        timer += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && timer >= waitTime)
        {
            SetNewDestination();
        }
    }

    private void SetNewDestination()
    {
        if (agent.isOnNavMesh)
        {
            int mask = NavMeshZoneManager.GetMask(allowedZone);
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, mask);
            agent.SetDestination(newPos);
            timer = 0;
            waitTime = Random.Range(minWaitTime, maxWaitTime);
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float radius, int mask, int maxAttempts = 20)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPoint = origin + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, mask))
            {
                return hit.position;
            }
        }
        return origin;
    }

    public void Activate()
    {
        active = true;
        SetNewDestination();
    }

    public void Deactivate()
    {
        active = false;
        agent.ResetPath();
    }

    public void SetZone(NavZone zone)
    {
        allowedZone = zone;
    }
}
