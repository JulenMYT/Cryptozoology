using UnityEngine;

public class AnimalResource : MonoBehaviour
{
    private float timer = 0f;
    private float time;
    private bool resourceReady = false;
    private ObjectData resource;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= time)
        {
            ProduceResource();
            timer = 0f;
        }
    }

    private void ProduceResource()
    {
        resourceReady = true;
        timer = 0;
    }

    public void SetResource(ObjectData resourceData, float productionTime)
    {
        resource = resourceData;
        time = productionTime;
    }
}
