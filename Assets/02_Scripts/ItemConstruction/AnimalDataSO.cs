using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimal", menuName = "Garden/Animal")]
public class AnimalDataSO : ObjectData
{
    public AnimalConditions conditions;
    public GameObject encyclopediaPrefab;
}
