using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField, Min(0f)] private float followSmoothTime = 0.15f;

    [Header("Zoom")]
    [SerializeField, Min(0.1f)] private float targetOrthographicSize = 4f;
    [SerializeField, Min(0f)] private float zoomSpeed = 5f;

    private Camera cachedCamera;
    private Vector3 followVelocity;

    private void Awake()
    {
        cachedCamera = GetComponent<Camera>();

        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            target = playerObject != null ? playerObject.transform : null;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followSmoothTime);

        if (cachedCamera != null && cachedCamera.orthographic)
        {
            cachedCamera.orthographicSize = Mathf.MoveTowards(
                cachedCamera.orthographicSize,
                targetOrthographicSize,
                zoomSpeed * Time.deltaTime);
        }
    }
}
