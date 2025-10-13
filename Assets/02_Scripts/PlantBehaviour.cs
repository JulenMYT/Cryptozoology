using Unity.Collections;
using UnityEngine;

public class PlantBehaviour : PlaceableObject, IEdible
{
    private PlantDataSO plantData;
    private int stage;
    private float timer;
    private float stageDuration;
    private int portionsLeft;
    private bool isMature = false;

    private PlantVisual visual;

    [ReadOnly]
    public PlantSaveData PlantSaveData = new();

    public override void Initialize(ObjectData data)
    {
        PlantSaveData.ID = SaveData.GenerateID();
        PlantSaveData.name = data.displayName;  

        PlantDataSO plantData = data as PlantDataSO;
        this.plantData = plantData;
        stage = 0;
        timer = 0f;
        portionsLeft = this.plantData.portions;
        stageDuration = this.plantData.totalGrowthTime / Mathf.Max(this.plantData.growthSprites.Length - 1, 1);

        visual = GetComponentInChildren<PlantVisual>();
        if (visual != null && this.plantData.growthSprites.Length > 0)
            visual.SetSprite(this.plantData.growthSprites[0]);
    }

    public override void Initialize(ObjectData objectData, PlaceableObjectSaveData saveData)
    {
        PlantDataSO plantData = objectData as PlantDataSO;
        this.plantData = plantData;
        stageDuration = this.plantData.totalGrowthTime / Mathf.Max(this.plantData.growthSprites.Length - 1, 1);

        PlantSaveData = saveData as PlantSaveData;

        stage = PlantSaveData.stage;
        timer = PlantSaveData.timer;
        portionsLeft = PlantSaveData.portionsLeft;
        isMature = PlantSaveData.isMature;

        visual = GetComponentInChildren<PlantVisual>();
        if (visual != null && this.plantData.growthSprites.Length > 0)
            visual.SetSprite(this.plantData.growthSprites[stage]);
    }

    private void Update()
    {
        if (!Placed)
        {
            return;
        }

        if (plantData == null || isMature)
            return;

        timer += Time.deltaTime;
        if (timer >= stageDuration)
        {
            timer = 0f;
            stage++;
            if (visual != null)
                visual.SetSprite(plantData.growthSprites[stage]);

            if (stage == plantData.growthSprites.Length - 1)
            {
                isMature = true;
            }
        }
    }

    public bool IsDepleted() => portionsLeft <= 0;

    public bool CanBeEaten()
    {
        return isMature;
    }

    public void Eat()
    {
        portionsLeft--;
        if (visual != null)
        {
            StartCoroutine(visual.SetColorFor(Color.blue, 1.8f));
        }
    }

    public string GetId()
    {
        return plantData.name;
    }

    protected override void Save()
    {
        PlantSaveData.position = transform.position;
        PlantSaveData.stage = stage;
        PlantSaveData.timer = timer;
        PlantSaveData.portionsLeft = portionsLeft;
        PlantSaveData.isMature = isMature;
        GameManager.Instance.SaveManager.saveData.AddData(PlantSaveData);
    }
}
