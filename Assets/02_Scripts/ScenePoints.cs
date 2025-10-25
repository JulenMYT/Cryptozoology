using System.Collections.Generic;
using UnityEngine;

public class ScenePoints : MonoBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints = new();
    [SerializeField] private WaypointPath _waypointPath;

    public List<Transform> SpawnPoints => _spawnPoints;
    public WaypointPath WaypointPath => _waypointPath;

}
