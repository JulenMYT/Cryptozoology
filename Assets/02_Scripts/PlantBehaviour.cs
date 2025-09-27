using UnityEngine;

public class PlantBehaviour : MonoBehaviour, IEdible
{
    private PlantDataSO data;
    private int stage;
    private float timer;
    private float stageDuration;
    private int portionsLeft;

    private PlantVisual visual;

    public void Initialize(PlantDataSO plantData)
    {
        data = plantData;
        stage = 0;
        timer = 0f;
        portionsLeft = data.portions;
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
            {
                ObjectEvents.OnObjectReplaced?.Invoke(data.id, data.matureId, gameObject);
            }
        }
    }

    public bool IsDepleted() => portionsLeft <= 0;

    public bool CanBeEaten()
    {
        return stage >= data.growthSprites.Length - 1 && portionsLeft > 0;
    }

    public void Eat()
    {
        portionsLeft--;
    }

    public string GetId()
    {
        return data != null ? data.id : string.Empty;
    }
}
