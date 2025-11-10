using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIDayNightClock : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite sunSprite;
    [SerializeField] private Sprite moonSprite;
    [SerializeField] private bool use24HourFormat = true;

    private static readonly string[] MonthAbbreviations =
    {
        "Jan", "Fév", "Mar", "Avr", "Mai", "Juin", "Juil", "Aoû", "Sep", "Oct", "Nov", "Déc"
    };

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.DayNight == null) return;

        var dayNight = GameManager.Instance.DayNight;

        int hours = dayNight.GetHour();
        int minutes = dayNight.GetMinute();
        int day = dayNight.Day;
        int month = Mathf.Clamp(dayNight.Month, 1, MonthAbbreviations.Length);
        int year = dayNight.Year;

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

        string dateString = string.Format("{0:00} {1} {2:000}", day, MonthAbbreviations[month - 1], year);

        timeText.text = timeString;
        dateText.text = dateString;

        if (iconImage != null && sunSprite != null && moonSprite != null)
        {
            iconImage.sprite = (hours >= 6 && hours < 18) ? sunSprite : moonSprite;
        }
    }
}
