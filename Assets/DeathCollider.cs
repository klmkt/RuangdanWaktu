using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCollider : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (sceneController != null)
            {
                sceneController.RefreshScene();
            }
        }
    }
}
