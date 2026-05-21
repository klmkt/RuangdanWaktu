using UnityEngine;

public class AnalogClock : MonoBehaviour
{
    [Header("Time Source")]
    [SerializeField] private TimeManager timeManager;
    [Tooltip("When enabled, the hands follow TimeManager. If no TimeManager is assigned, the clock uses its local time.")]
    [SerializeField] private bool autoFromTime;
    [Tooltip("When enabled, ignore TimeManager and always use local time for this clock.")]
    [SerializeField] private bool forceLocalTime;

    [Header("Starting Time")]
    [Tooltip("Initial time for manual mode and for seeding TimeManager when Auto From Time is enabled.")]
    [SerializeField, Range(1, 12)] private int startingHour = 12;
    [SerializeField, Range(0, 59)] private int startingMinute = 0;
    [Tooltip("When enabled, applies the starting time to TimeManager on Awake.")]
    [SerializeField] private bool applyStartTimeToTimeManager = true;

    [Header("Playback Speed (when Auto Advance is ON)")]
    [SerializeField] private float dayDuration = 30f;
    [SerializeField] private float playbackSpeed = 1f;

    [Header("Player Proximity Freeze")]
    [Tooltip("When enabled, the hands stop updating while the player is inside the clock sprite bounds.")]
    [SerializeField] private bool freezeWhenPlayerInside;
    [Tooltip("Optional. If left empty, uses the SpriteRenderer on this GameObject.")]
    [SerializeField] private SpriteRenderer clockSprite;
    [Tooltip("Optional. If left empty, uses the GameObject tagged Player.")]
    [SerializeField] private Transform player;

    [Header("Player Proximity Fade")]
    [SerializeField, Range(0f, 1f)] private float insideOpacity = 0.2f;
    [SerializeField, Range(0f, 1f)] private float outsideOpacity = 0.5f;
    [SerializeField] private float opacityFadeSpeed = 2f;

    [Header("References")]
    [SerializeField] private Transform clockCenter;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Transform hourHand;

    private const float HoursToDegrees = 360f / 12f;     // 30° per hour
    private const float MinutesToDegrees = 360f / 60f;   // 6° per minute
    private const float TwelveOClockAngle = 90f;         // 90° = upward in Unity 2D
    private const float MinutesInDay = 24f * 60f;

    private Vector3 minuteHandOffset;
    private Vector3 hourHandOffset;
    private bool offsetsCached;
    private Rigidbody2D minuteHandBody;
    private Rigidbody2D hourHandBody;
    private float hourAngle;
    private float minuteAngle;
    private HandSelection activeHand;
    private bool isFrozen;
    private float timeOffsetMinutes;
    private float frozenTotalMinutes;


    // Independent time tracking
    private float independentTotalTime;

    private enum HandSelection
    {
        None,
        Hour,
        Minute
    }

    private void Awake()
    {
        ResolveClockCenter();

        if (timeManager == null && !forceLocalTime)
        {
            timeManager = FindFirstObjectByType<TimeManager>();
        }

        ResolvePlayer();
        ResolveClockSprite();
        CacheBodies();
        CacheOffsets();
        InitializeTime();
    }

    private void InitializeTime()
    {
        if (autoFromTime && timeManager != null && !forceLocalTime)
        {
            if (applyStartTimeToTimeManager)
            {
                timeManager.SetTime(startingHour, startingMinute);
            }

            // Will sync with TimeManager in Update
            return;
        }

        // Initialize independent time tracking
        float totalMinutes = Mathf.Clamp(startingHour, 0, 23) * 60f + Mathf.Clamp(startingMinute, 0, 59);
        independentTotalTime = totalMinutes * dayDuration / MinutesInDay;
    }

    private void OnValidate()
    {
        ResolveClockCenter();
        CacheBodies();
        CacheOffsets();
        ResolveClockSprite();
    }

    private void ResolveClockCenter()
    {
        Transform namedCenter = transform.Find("ClockCenter");

        if (clockCenter == null)
        {
            clockCenter = namedCenter != null ? namedCenter : transform;
            return;
        }

        if (clockCenter == transform && namedCenter != null)
        {
            clockCenter = namedCenter;
        }
    }

    private void Update()
    {
        UpdateClockOpacity(IsPlayerInsideClock());

        if (hourHand == null || minuteHand == null)
        {
            return;
        }

        HandleMouseInput();

        if (activeHand != HandSelection.None)
        {
            // User is dragging, don't update time
            return;
        }

        bool shouldFreeze = ShouldFreezeClock();

        if (shouldFreeze)
        {
            BeginFreezeIfNeeded();
            return;
        }
        else if (isFrozen)
        {
            EndFreeze();
        }

        if (TryGetTimeFromManager(out float hourValue, out float minuteValue))
        {
            UpdateAngles(hourValue, minuteValue);
            return;
        }

        AdvanceIndependentTimeIfNeeded();
        GetIndependentTime(out hourValue, out minuteValue);
        UpdateAngles(hourValue, minuteValue);
    }

    private void FixedUpdate()
    {
        if (hourHand == null || minuteHand == null)
        {
            return;
        }

        if (clockCenter == null)
        {
            ApplyRotationOnly(hourHand, hourHandBody, hourAngle);
            ApplyRotationOnly(minuteHand, minuteHandBody, minuteAngle);
            return;
        }

        if (!offsetsCached)
        {
            CacheOffsets();
        }

        ApplyAroundCenter(hourHand, hourHandBody, hourAngle, hourHandOffset);
        ApplyAroundCenter(minuteHand, minuteHandBody, minuteAngle, minuteHandOffset);
    }

    private void CacheOffsets()
    {
        if (clockCenter == null || hourHand == null || minuteHand == null)
        {
            offsetsCached = false;
            return;
        }

        hourHandOffset = hourHand.position - clockCenter.position;
        minuteHandOffset = minuteHand.position - clockCenter.position;
        offsetsCached = true;
    }

    private void CacheBodies()
    {
        hourHandBody = hourHand != null ? hourHand.GetComponent<Rigidbody2D>() : null;
        minuteHandBody = minuteHand != null ? minuteHand.GetComponent<Rigidbody2D>() : null;
    }

    private void ResolveClockSprite()
    {
        if (clockSprite != null)
        {
            return;
        }

        clockSprite = GetComponent<SpriteRenderer>();
    }

    private void ResolvePlayer()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
    }

    private bool ShouldFreezeClock()
    {
        if (!freezeWhenPlayerInside)
        {
            return false;
        }

        return IsPlayerInsideClock();
    }

    private bool IsPlayerInsideClock()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 centerPosition = clockCenter != null ? clockCenter.position : transform.position;
        float radius = 0f;

        if (clockSprite != null)
        {
            Bounds bounds = clockSprite.bounds;
            centerPosition = bounds.center;
            radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
        }

        if (radius <= 0f)
        {
            return false;
        }

        Vector2 delta = (Vector2)(player.position - centerPosition);
        return delta.sqrMagnitude <= radius * radius;
    }

    private void UpdateClockOpacity(bool playerInside)
    {
        if (clockSprite == null)
        {
            return;
        }

        float targetAlpha = playerInside ? insideOpacity : outsideOpacity;
        Color color = clockSprite.color;
        float newAlpha = Mathf.MoveTowards(color.a, targetAlpha, opacityFadeSpeed * Time.deltaTime);

        if (!Mathf.Approximately(color.a, newAlpha))
        {
            color.a = newAlpha;
            clockSprite.color = color;
        }
    }

    private bool TryGetTimeFromManager(out float hourValue, out float minuteValue)
    {
        if (!autoFromTime || forceLocalTime || timeManager == null)
        {
            hourValue = 0f;
            minuteValue = 0f;
            return false;
        }

        float adjustedMinutes = GetAdjustedTotalMinutes();
        hourValue = (adjustedMinutes / 60f) % 12f;
        minuteValue = adjustedMinutes % 60f;
        return true;
    }

    private void AdvanceIndependentTimeIfNeeded()
    {
        if (!autoFromTime)
        {
            return;
        }

        // Use independent time tracking when no TimeManager is assigned.
        independentTotalTime += Time.deltaTime * playbackSpeed;

        if (independentTotalTime >= dayDuration)
        {
            independentTotalTime = Mathf.Repeat(independentTotalTime, dayDuration);
        }
    }

    private void GetIndependentTime(out float hourValue, out float minuteValue)
    {
        hourValue = (independentTotalTime * 24f / dayDuration) % 12f;
        minuteValue = (independentTotalTime * 24f * 60f / dayDuration) % 60f;
    }

    private void UpdateAngles(float hourValue, float minuteValue)
    {
        hourAngle = TwelveOClockAngle - (hourValue * HoursToDegrees);
        minuteAngle = TwelveOClockAngle - (minuteValue * MinutesToDegrees);
    }

    private float GetManagerTotalMinutes()
    {
        if (timeManager == null)
        {
            return 0f;
        }

        // GetHour() already includes fractional minutes, so multiply once to avoid double counting.
        return timeManager.GetHour() * 60f;
    }

    private float GetAdjustedTotalMinutes()
    {
        float managerMinutes = GetManagerTotalMinutes();
        return Mathf.Repeat(managerMinutes + timeOffsetMinutes, MinutesInDay);
    }

    private void BeginFreezeIfNeeded()
    {
        if (isFrozen)
        {
            return;
        }

        if (autoFromTime && timeManager != null && !forceLocalTime)
        {
            frozenTotalMinutes = GetAdjustedTotalMinutes();
        }

        isFrozen = true;
    }

    private void EndFreeze()
    {
        if (autoFromTime && timeManager != null && !forceLocalTime)
        {
            float managerMinutes = GetManagerTotalMinutes();
            timeOffsetMinutes = frozenTotalMinutes - managerMinutes;
        }

        isFrozen = false;
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            activeHand = PickHandAtMouse();
        }

        if (activeHand != HandSelection.None && Input.GetMouseButton(0))
        {
            UpdateDraggedHandAngle(activeHand);
        }

        if (Input.GetMouseButtonUp(0))
        {
            activeHand = HandSelection.None;
        }
    }

    private HandSelection PickHandAtMouse()
    {
        Camera cameraToUse = Camera.main;

        if (cameraToUse == null)
        {
            return HandSelection.None;
        }

        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit == null)
        {
            return HandSelection.None;
        }

        Transform hitTransform = hit.transform;

        if (minuteHand != null && (hitTransform == minuteHand || hitTransform.IsChildOf(minuteHand)))
        {
            return HandSelection.Minute;
        }

        if (hourHand != null && (hitTransform == hourHand || hitTransform.IsChildOf(hourHand)))
        {
            return HandSelection.Hour;
        }

        return HandSelection.None;
    }

    private void UpdateDraggedHandAngle(HandSelection hand)
    {
        Camera cameraToUse = Camera.main;

        if (cameraToUse == null)
        {
            return;
        }

        Vector3 mouseWorld = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        Vector3 centerPosition = clockCenter != null ? clockCenter.position : transform.position;
        Vector2 direction = mouseWorld - centerPosition;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float draggedAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (hand == HandSelection.Hour)
        {
            hourAngle = draggedAngle;
            return;
        }

        minuteAngle = draggedAngle;
    }

    private static void ApplyRotationOnly(Transform hand, Rigidbody2D handBody, float angle)
    {
        if (handBody != null)
        {
            handBody.MoveRotation(angle);
            return;
        }

        hand.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ApplyAroundCenter(Transform hand, Rigidbody2D handBody, float angle, Vector3 offset)
    {
        Vector3 targetPosition = clockCenter.position + Quaternion.AngleAxis(angle, Vector3.forward) * offset;

        if (handBody != null)
        {
            handBody.MovePosition(targetPosition);
            handBody.MoveRotation(angle);
            return;
        }

        hand.position = targetPosition;
        hand.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
