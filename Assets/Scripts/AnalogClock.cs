using UnityEngine;

public class AnalogClock : MonoBehaviour
{
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private bool autoFromTime;
    [SerializeField] private Transform clockCenter;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Transform hourHand;

    private const float HoursToDegrees = 360f / 12f;
    private const float MinutesToDegrees = 360f / 60f;
    private Vector3 minuteHandOffset;
    private Vector3 hourHandOffset;
    private bool offsetsCached;
    private Rigidbody2D minuteHandBody;
    private Rigidbody2D hourHandBody;
    private float hourAngle;
    private float minuteAngle;
    private HandSelection activeHand;

    private enum HandSelection
    {
        None,
        Hour,
        Minute
    }

    private void Awake()
    {
        if (timeManager == null)
        {
            timeManager = FindObjectOfType<TimeManager>();
        }

        CacheBodies();
        CacheOffsets();
        SyncAnglesFromHands();
    }

    private void OnValidate()
    {
        CacheBodies();
        CacheOffsets();
    }

    private void Update()
    {
        if (hourHand == null || minuteHand == null)
        {
            return;
        }

        HandleMouseInput();

        if (autoFromTime && activeHand == HandSelection.None && timeManager != null)
        {
            hourAngle = -timeManager.GetHour() * HoursToDegrees;
            minuteAngle = -timeManager.GetMinutes() * MinutesToDegrees;
        }
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

    private void SyncAnglesFromHands()
    {
        if (hourHand != null)
        {
            hourAngle = Mathf.DeltaAngle(0f, hourHand.eulerAngles.z);
        }

        if (minuteHand != null)
        {
            minuteAngle = Mathf.DeltaAngle(0f, minuteHand.eulerAngles.z);
        }
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
