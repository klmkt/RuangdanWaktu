using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    private float length;
    private float startPosX;
    private float startPosY;

    [SerializeField] private Camera cam;
    [SerializeField] private float parallaxEffect = 0.5f;

    private void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            length = spriteRenderer.bounds.size.x;
        }
    }

    private void FixedUpdate()
    {
        if (cam == null)
        {
            return;
        }

        float dist = cam.transform.position.x * parallaxEffect;
        float temp = cam.transform.position.x * (1f - parallaxEffect);

        transform.position = new Vector3(startPosX + dist, cam.transform.position.y - 1f, transform.position.z);

        if (temp > startPosX + length)
        {
            startPosX += length;
        }
        else if (temp < startPosX - length)
        {
            startPosX -= length;
        }
    }
}
