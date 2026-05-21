using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementTimeBridge : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private float minutesPerWorldUnit = 20f;
    [Tooltip("Saat aktif, waktu mengikuti pergerakan player dan Auto Advance di TimeManager akan dimatikan.")]
    [SerializeField] private bool usePlayerMovementTime = false;

    [Header("Starting Time")]
    [Tooltip("When enabled, applies the starting time to TimeManager while player movement time is used.")]
    [SerializeField] private bool applyStartTime = false;
    [SerializeField, Range(1, 12)] private int startingHour = 12;
    [SerializeField, Range(0, 59)] private int startingMinute = 0;

    private Rigidbody2D rb2d;
    private float previousX;
    private bool startTimeDirty;
    private bool inspectorCacheInitialized;
    private int lastStartingHour;
    private int lastStartingMinute;
    private bool lastApplyStartTime;

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
        ApplyStartTimeIfDirty(true);
        CacheInspectorValues();
    }

    private void Update()
    {
        ApplyStartTimeIfDirty(false);
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

    private void OnValidate()
    {
        bool shouldMarkDirty = false;

        if (inspectorCacheInitialized)
        {
            shouldMarkDirty = startingHour != lastStartingHour
                || startingMinute != lastStartingMinute
                || applyStartTime != lastApplyStartTime;
        }

        CacheInspectorValues();

        if (Application.isPlaying && shouldMarkDirty)
        {
            startTimeDirty = true;
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

    private void ApplyStartTimeIfDirty(bool forceApply)
    {
        if (timeManager == null)
        {
            return;
        }

        if (!applyStartTime)
        {
            startTimeDirty = false;
            return;
        }

        if (!forceApply && !startTimeDirty)
        {
            return;
        }

        timeManager.SetTime(startingHour, startingMinute);
        startTimeDirty = false;
    }

    private void CacheInspectorValues()
    {
        lastStartingHour = startingHour;
        lastStartingMinute = startingMinute;
        lastApplyStartTime = applyStartTime;
        inspectorCacheInitialized = true;
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
