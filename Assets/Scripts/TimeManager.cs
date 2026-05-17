using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public const int hoursInDay = 24, minutesInHour = 60;

    [SerializeField] private float dayDuration = 30f;
<<<<<<< Updated upstream
=======
    [SerializeField] private bool autoAdvance = true;
    [SerializeField, Range(0, 12)] private int startHour;
    [SerializeField, Range(0, minutesInHour - 1)] private int startMinute;
>>>>>>> Stashed changes

    private float totalTime;
    private float currentTime;

    private void Awake()
    {
        ApplyStartTime();
    }

    private void OnValidate()
    {
        ApplyStartTime();
    }

    private void Update()
    {
        if (dayDuration <= 0f)
        {
            return;
        }

        totalTime += Time.deltaTime;
        currentTime = totalTime % dayDuration;
    }

    private void ApplyStartTime()
    {
        if (dayDuration <= 0f)
        {
            totalTime = 0f;
            currentTime = 0f;
            return;
        }

        float normalizedHour = startHour % 12;
        float startMinutesInDay = normalizedHour * minutesInHour + startMinute;
        float startSeconds = startMinutesInDay * dayDuration / MinutesInDay;
        totalTime = Mathf.Repeat(startSeconds, dayDuration);
        currentTime = totalTime;
    }

    public float GetHour()
    {
        return currentTime * hoursInDay / dayDuration;
    }

    public float GetMinutes()
    {
        return (currentTime * hoursInDay * minutesInHour / dayDuration) % minutesInHour;
    }

    public string Clock24Hour()
    {
        return Mathf.FloorToInt(GetHour()).ToString("00") + ":" + Mathf.FloorToInt(GetMinutes()).ToString("00");
    }

    public string Clock12Hour()
    {
        int hour = Mathf.FloorToInt(GetHour());
        string abbreviation = "AM";

        if (hour >= 12)
        {
            abbreviation = "PM";
            hour -= 12;
        }

        if (hour == 0) hour = 12;

        return hour.ToString("00") + ":" + Mathf.FloorToInt(GetMinutes()).ToString("00") + " " + abbreviation;
    }
}
