using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalConditions", menuName = "Scriptable Objects/AnimalConditions")]
public class AnimalConditions : ScriptableObject
{
    [System.Serializable]
    public class Condition
    {
        public string id;
        public int minCount;
    }

    [System.Serializable]
    public class ConditionList
    {
        public List<Condition> placingConditions = new();
        public List<Condition> eatingConditions = new();
    }

    public ConditionList appearingCondition;
    public ConditionList visitCondition;
    public ConditionList residenceCondition;

    public bool CanAppear()
    {
        foreach (var condition in appearingCondition.placingConditions)
        {
            if (GameManager.Instance.Garden.GetCount(condition.id) < condition.minCount)
            {
                return false;
            }
        }
        return true;
    }
}
