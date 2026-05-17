using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public const int hoursInDay = 24, minutesInHour = 60;
    private const float minutesInDay = hoursInDay * minutesInHour;

    [SerializeField] private float dayDuration = 30f;
    [SerializeField] private bool autoAdvance = true;

    private float totalTime;
    private float currentTime;

    private void Update()
    {
        if (!autoAdvance || dayDuration <= 0f)
        {
            return;
        }

        AddSeconds(Time.deltaTime);
    }

    public void SetAutoAdvance(bool enabled)
    {
        autoAdvance = enabled;
    }

    public void AddMinutes(float deltaMinutes)
    {
        if (dayDuration <= 0f)
        {
            return;
        }

        float deltaSeconds = deltaMinutes * dayDuration / minutesInDay;
        AddSeconds(deltaSeconds);
    }

    private void AddSeconds(float deltaSeconds)
    {
        totalTime = Mathf.Repeat(totalTime + deltaSeconds, dayDuration);
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
