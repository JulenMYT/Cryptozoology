using UnityEngine;

[CreateAssetMenu(fileName = "NewPlant", menuName = "Garden/Plant")]
public class PlantDataSO : ObjectData
{
    [Header("Plant Growth")]
    public float totalGrowthTime;
    public Sprite[] growthSprites;
    public string matureId;
    public int portions;
}
