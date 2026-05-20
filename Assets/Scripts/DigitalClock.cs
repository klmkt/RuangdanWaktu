using UnityEngine;
using UnityEngine.UI;

public class DigitalClock : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private bool use24HourClock = true;

    private Text display;

    private void Awake()
    {
        display = GetComponent<Text>();

        if (timeManager == null)
        {
            timeManager = FindFirstObjectByType<TimeManager>();
        }
    }

    private void Update()
    {
        if (timeManager == null || display == null)
        {
            return;
        }

        display.text = use24HourClock ? timeManager.Clock24Hour() : timeManager.Clock12Hour();
    }
}
