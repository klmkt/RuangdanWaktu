using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementTimeBridge : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private float minutesPerWorldUnit = 20f;
    [Tooltip("Saat aktif, waktu mengikuti pergerakan player dan Auto Advance di TimeManager akan dimatikan.")]
    [SerializeField] private bool usePlayerMovementTime = false;

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
        SyncAutoAdvance();
    }

    private void OnEnable()
    {
        SyncAutoAdvance();
    }

    private void OnDisable()
    {
        if (timeManager != null)
        {
            timeManager.SetAutoAdvance(true);
        }
    }

    private void FixedUpdate()
    {
        float currentX = rb2d.position.x;

        if (!usePlayerMovementTime || timeManager == null || Mathf.Approximately(minutesPerWorldUnit, 0f))
        {
            previousX = currentX;
            return;
        }

        float deltaX = currentX - previousX;
        previousX = currentX;

        if (Mathf.Approximately(deltaX, 0f))
        {
            return;
        }

        timeManager.AddMinutes(deltaX * minutesPerWorldUnit);
    }

    private void SyncAutoAdvance()
    {
        if (timeManager == null)
        {
            return;
        }

        timeManager.SetAutoAdvance(!usePlayerMovementTime);
    }
}
