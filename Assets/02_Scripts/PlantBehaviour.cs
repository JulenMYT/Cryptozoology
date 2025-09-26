using UnityEngine;

public class PlantBehaviour : MonoBehaviour
{
    private PlantDataSO data;
    private int stage;
    private float timer;
    private float stageDuration;

    private PlantVisual visual;

    public void Initialize(PlantDataSO plantData)
    {
        data = plantData;
        stage = 0;
        timer = 0f;
        stageDuration = data.totalGrowthTime / Mathf.Max(data.growthSprites.Length - 1, 1);

        visual = GetComponentInChildren<PlantVisual>();
        if (visual != null && data.growthSprites.Length > 0)
            visual.SetSprite(data.growthSprites[0]);
    }

    private void Update()
    {
        if (data == null || stage >= data.growthSprites.Length - 1)
            return;

        timer += Time.deltaTime;
        if (timer >= stageDuration)
        {
            timer = 0f;
            stage++;
            if (visual != null)
                visual.SetSprite(data.growthSprites[stage]);

            if (stage == data.growthSprites.Length - 1 && !string.IsNullOrEmpty(data.matureId))
                ItemEvents.OnItemReplaced?.Invoke(data.id, data.matureId, gameObject);
        }
    }
}
