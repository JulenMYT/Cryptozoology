using System;
using UnityEngine;

public static class ObjectEvents
{
    public static Action<string, GameObject> OnObjectAdded;
    public static Action<string, GameObject> OnObjectRemoved;
    public static Action<string, string, GameObject> OnObjectReplaced;
}
