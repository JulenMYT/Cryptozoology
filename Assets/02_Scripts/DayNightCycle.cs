using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    [SerializeField] private float dayLengthInSeconds = 300f;
    [SerializeField] private float initialTimeOfDay = 0f;
    
    private float timeOfDay;

    public float TimeOfDay => timeOfDay;
    public float DayLengthInSeconds => dayLengthInSeconds;

    private void Start()
    {
        timeOfDay = Mathf.Clamp01(initialTimeOfDay);
    }

    private void Update()
    {
        timeOfDay += Time.deltaTime / dayLengthInSeconds;
        if (timeOfDay > 1f) timeOfDay -= 1f;
    }

    public void SetTimeOfDay(float normalizedTime)
    {
        timeOfDay = Mathf.Clamp01(normalizedTime);
    }

    public void PauseCycle() => enabled = false;
    public void ResumeCycle() => enabled = true;

    public int GetHour()
    {
        return Mathf.FloorToInt(timeOfDay * 24f);
    }

    public int GetMinute()
    {
        return Mathf.FloorToInt((timeOfDay * 24f - GetHour()) * 60f);
    }
}
