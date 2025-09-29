using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimalPatrol : MonoBehaviour, IAnimalBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    private WaypointPath waypointPath;

    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTimeAtWaypoint = 0f;
    [SerializeField] private bool randomizeDirection = true;
    [SerializeField] private float stoppingDistance = 1f;

    private List<Transform> waypoints = new();
    private int currentIndex = 0;
    private bool forward = true;
    private bool active = false;
    private float timer = 0f;

    public bool IsActive() => active;

    private void Awake()
    {
        if (waypointPath != null)
            waypoints.AddRange(waypointPath.waypoints);
    }

    private void Update()
    {
        if (!active || waypoints.Count == 0 || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= stoppingDistance)
        {
            timer += Time.deltaTime;
            if (timer >= waitTimeAtWaypoint)
            {
                timer = 0f;
                GoToNextWaypoint();
            }
        }
    }

    public void Activate()
    {
        if (waypoints.Count == 0) return;

        float minDist = float.MaxValue;
        for (int i = 0; i < waypoints.Count; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                currentIndex = i;
            }
        }

        forward = randomizeDirection ? (Random.value > 0.5f) : true;

        active = true;
        agent.SetDestination(waypoints[currentIndex].position);
    }

    public void Deactivate()
    {
        active = false;
        agent.ResetPath();
    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Count <= 1) return;

        if (forward)
        {
            currentIndex++;
            if (currentIndex >= waypoints.Count)
            {
                if (loop) currentIndex = 0;
                else { currentIndex = waypoints.Count - 1; forward = false; }
            }
        }
        else
        {
            currentIndex--;
            if (currentIndex < 0)
            {
                if (loop) currentIndex = waypoints.Count - 1;
                else { currentIndex = 0; forward = true; }
            }
        }

        agent.SetDestination(waypoints[currentIndex].position);
    }

    public void SetWaypoints(WaypointPath path)
    {
        waypointPath = path;
        waypoints.Clear();
        if (waypointPath != null)
            waypoints.AddRange(waypointPath.waypoints);
    }
}
