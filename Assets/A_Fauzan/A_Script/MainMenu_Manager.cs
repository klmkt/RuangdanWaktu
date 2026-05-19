using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Hanya butuh  ingsUI untuk saat ini sesuai Hierarchy-mu
    [SerializeField] private SettingsUIManage   r _settingsUI;

    // Pasangkan fungsi ini ke event OnClick tombol 'Start'
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Pindah ke scene: " + sceneName);
    }

    // Pasangkan fungsi ini ke event OnClick tombol 'Settings'
    public void SettingsButton()
    {
        Debug.Log("Tombol Settings Diklik!");
        if (_settingsUI != null)
        {
            _settingsUI.OpenSettings();
        }
        else
        {
            Debug.LogWarning("SettingsUI belum di-assign di Inspector MainMenuManager!");
        }
    }

    // Pasangkan fungsi ini ke event OnClick tombol 'Quit'
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game dipanggil (Hanya terlihat efeknya saat sudah di-Build)");
    }

    // Biarkan fungsi ini jika project-mu sudah memiliki AudioManager
    public void StartSFXButton()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX("Button Click");
        }
    }
}