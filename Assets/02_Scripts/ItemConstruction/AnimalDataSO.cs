using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimal", menuName = "Garden/Animal")]
public class AnimalDataSO : ObjectData
{
    [Header("Plant Growth")]
    public AnimalConditions conditions;
}
