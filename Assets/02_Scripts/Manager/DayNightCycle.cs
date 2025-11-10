using UnityEngine;

public class DayNightCycleManager : MonoBehaviour
{
    [SerializeField] private float dayLengthInSeconds = 300f;
    [SerializeField] private float initialTimeOfDay = 0f;
    [SerializeField] private int startDay = 29;
    [SerializeField] private int startMonth = 10;
    [SerializeField] private int startYear = 1802;

    private float timeOfDay;
    private int day;
    private int month;
    private int year;

    private static readonly int[] daysInMonths =
        { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    public float TimeOfDay => timeOfDay;
    public float DayLengthInSeconds => dayLengthInSeconds;
    public int Day => day;
    public int Month => month;
    public int Year => year;


    private void Awake()
    {
        timeOfDay = Mathf.Clamp01(initialTimeOfDay);
        day = startDay;
        month = startMonth;
        year = startYear;
    }

    private void Start()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnSave += Save;
    }

    private void OnDisable()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.OnSave -= Save;
    }

    private void Update()
    {
        timeOfDay += Time.deltaTime / dayLengthInSeconds;
        if (timeOfDay >= 1f)
        {
            timeOfDay -= 1f;
            AdvanceDay();
        }
    }

    private void AdvanceDay()
    {
        day++;
        int daysThisMonth = GetDaysInMonth(month, year);
        if (day > daysThisMonth)
        {
            day = 1;
            month++;
            if (month > 12)
            {
                month = 1;
                year++;
            }
        }
    }

    private int GetDaysInMonth(int m, int y)
    {
        if (m == 2)
            return IsLeapYear(y) ? 29 : 28;
        return daysInMonths[m - 1];
    }

    private bool IsLeapYear(int y)
    {
        return (y % 4 == 0 && y % 100 != 0) || (y % 400 == 0);
    }

    public void SetTimeOfDay(float t) => timeOfDay = Mathf.Clamp01(t);

    public void SetDate(int d, int m, int y)
    {
        day = Mathf.Clamp(d, 1, GetDaysInMonth(m, y));
        month = Mathf.Clamp(m, 1, 12);
        year = Mathf.Max(1, y);
    }

    public int GetHour() => Mathf.FloorToInt(timeOfDay * 24f);
    public int GetMinute() => Mathf.FloorToInt((timeOfDay * 24f - GetHour()) * 60f);

    public void Load(DayNightSaveData data)
    {
        if (data == null)
        {
            ResetToDefaults();
            return;
        }

        timeOfDay = Mathf.Clamp01(data.timeOfDay);
        month = Mathf.Clamp(data.month <= 0 ? startMonth : data.month, 1, 12);
        year = data.year <= 0 ? startYear : data.year;
        day = Mathf.Clamp(data.day <= 0 ? startDay : data.day, 1, GetDaysInMonth(month, year));
    }

    private void ResetToDefaults()
    {
        timeOfDay = Mathf.Clamp01(initialTimeOfDay);
        month = startMonth;
        year = startYear;
        day = Mathf.Clamp(startDay, 1, GetDaysInMonth(month, year));
    }

    public void Save()
    {
        if (SaveManager.Instance == null || SaveManager.Instance.saveData == null) return;

        SaveManager.Instance.saveData.dayNightSaveData = new DayNightSaveData
        {
            timeOfDay = timeOfDay,
            day = day,
            month = month,
            year = year
        };
    }
}
