using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimal", menuName = "Garden/Animal")]
public class AnimalDataSO : ObjectData
{
    [Header("Plant Growth")]
    public AnimalConditions conditions;

    public GameObject encyclopediaPrefab;
    public string houseID;
    public float wakeUpTime;
    public float sleepTime;

    public bool ShouldSleep()
    {
        float time = GameManager.Instance.DayNight.GetHour();
        if (wakeUpTime < sleepTime)
        {
            return time < wakeUpTime || time >= sleepTime;
        }
        else
        {
            return time >= sleepTime && time < wakeUpTime;
        }
    }

    public bool ShouldSleep(float time)
    {
        if (wakeUpTime < sleepTime)
        {
            return time < wakeUpTime || time >= sleepTime;
        }
        else
        {
            return time >= sleepTime && time < wakeUpTime;
        }
    }
}
