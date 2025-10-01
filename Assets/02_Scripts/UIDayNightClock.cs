using UnityEngine;
using TMPro;

public class UIDayNightClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private bool use24HourFormat = true;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.DayNight == null) return;

        int hours = GameManager.Instance.DayNight.GetHour();
        int minutes = GameManager.Instance.DayNight.GetMinute();

        string timeString;
        if (use24HourFormat)
        {
            timeString = string.Format("{0:00}:{1:00}", hours, minutes);
        }
        else
        {
            int displayHour = hours % 12;
            if (displayHour == 0) displayHour = 12;
            string ampm = hours >= 12 ? "PM" : "AM";
            timeString = string.Format("{0:00}:{1:00} {2}", displayHour, minutes, ampm);
        }

        timeText.text = timeString;
    }
}
