using System;
using UnityEngine;

public static class ItemEvents
{
    public static Action<string, GameObject> OnItemAdded;
    public static Action<string, string, GameObject> OnItemReplaced;
}
