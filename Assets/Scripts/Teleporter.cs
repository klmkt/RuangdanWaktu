using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Teleporter : MonoBehaviour
{
    [Header("Scene Target")]
    [SerializeField] private string targetSceneName;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private bool _isTeleporting;

    private void Awake()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null && !collider2D.isTrigger)
        {
            Debug.LogWarning("Teleporter collider should be set to Trigger.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isTeleporting || other == null)
        {
            return;
        }

        if (!IsPlayer(other))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("Teleporter target scene name is empty.", this);
            return;
        }

        _isTeleporting = true;
        SceneManager.LoadScene(targetSceneName);
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            return true;
        }

        return other.GetComponent<PlayerMovement2D>() != null;
    }
}
