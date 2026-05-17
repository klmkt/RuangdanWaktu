using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementTimeBridge : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private float minutesPerWorldUnit = 20f;
    [SerializeField] private bool disableTimeManagerAutoAdvance = true;

    private Rigidbody2D rb2d;
    private float previousX;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

        if (timeManager == null)
        {
            timeManager = FindFirstObjectByType<TimeManager>();
        }
    }

    private void Start()
    {
        previousX = rb2d.position.x;

        if (disableTimeManagerAutoAdvance && timeManager != null)
        {
            timeManager.SetAutoAdvance(false);
        }
    }

    private void FixedUpdate()
    {
        if (timeManager == null || Mathf.Approximately(minutesPerWorldUnit, 0f))
        {
            previousX = rb2d.position.x;
            return;
        }

        float currentX = rb2d.position.x;
        float deltaX = currentX - previousX;
        previousX = currentX;

        if (Mathf.Approximately(deltaX, 0f))
        {
            return;
        }

        timeManager.AddMinutes(deltaX * minutesPerWorldUnit);
    }
}
