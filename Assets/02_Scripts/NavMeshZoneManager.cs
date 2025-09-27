using UnityEngine;
using UnityEngine.AI;

[System.Flags]
public enum NavZone
{
    None = 0,
    Garden = 1 << 0,
    Outside = 1 << 1,
    All = Garden | Outside
}

public static class NavMeshZoneManager
{
    public static int GetMask(NavZone zone)
    {
        int gardenIndex = NavMesh.GetAreaFromName("Garden");
        int walkableIndex = NavMesh.GetAreaFromName("Walkable");

        int mask = 0;

        if ((zone & NavZone.Garden) != 0)
            mask |= 1 << gardenIndex;
        if ((zone & NavZone.Outside) != 0)
        {
            int allMask = 1 << walkableIndex;
            int gardenMask = 1 << gardenIndex;
            mask |= allMask & ~gardenMask;
        }

        if (zone == NavZone.All)
            mask = (1 << walkableIndex) | (1 << gardenIndex);

        return mask;
    }

    public static void SetAgentZone(NavMeshAgent agent, NavZone zone)
    {
        agent.areaMask = GetMask(zone);
    }

    public static void AddZoneToAgent(NavMeshAgent agent, NavZone zone)
    {
        agent.areaMask |= GetMask(zone);
    }

    public static void RemoveZoneFromAgent(NavMeshAgent agent, NavZone zone)
    {
        agent.areaMask &= ~GetMask(zone);
    }
}
