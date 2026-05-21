using UnityEngine;

public class PortalAnim : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private GameObject[] portalParts;

    private void Update()
    {
        if (portalParts == null || portalParts.Length == 0)
        {
            return;
        }

        float countScale = 1f / Mathf.Max(1f, Mathf.Sqrt(portalParts.Length));
        for (int i = 0; i < portalParts.Length; i++)
        {
            GameObject part = portalParts[i];
            if (part == null)
            {
                continue;
            }

            float speed = rotationSpeed * countScale;
            part.transform.Rotate(0f, 0f, speed * Time.deltaTime, Space.Self);
        }
    }


}
