using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI Containers")]
    [SerializeField] private GameObject _pauseMenuContainer;
    [SerializeField] private CanvasGroup _backgroundOverlay;
    [SerializeField] private RectTransform _pausePanel;

    [Header("Animation Settings")]
    [SerializeField] private float _fadeDuration = 0.4f;
    [SerializeField] private float _panelDuration = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _targetBgAlpha = 0.8f;
    [Space]
    [SerializeField] private Ease _appearEase = Ease.OutBack;
    [SerializeField] private Ease _disappearEase = Ease.InBack;

    [Header("Buttons Reference")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;

    private bool _isPaused = false;

    [Header("2D Scripts References")]
    private PlayerMovement2D _playerMovement;
    private AnalogClock _analogClock;

    private void Awake()
    {
        Instance = this;

        // Inisialisasi awal agar UI tersembunyi
        if (_pauseMenuContainer != null) _pauseMenuContainer.SetActive(false);
        if (_backgroundOverlay != null) _backgroundOverlay.alpha = 0;
        if (_pausePanel != null) _pausePanel.localScale = Vector3.zero;

        // FIX WARNING: Menggunakan FindFirstObjectByType untuk Unity 2023+
        _playerMovement = Object.FindFirstObjectByType<PlayerMovement2D>();
        _analogClock = Object.FindFirstObjectByType<AnalogClock>();

        // AUTO-LINK BUTTONS: Menghubungkan fungsi ke tombol lewat script
        // Jadi kamu tidak perlu repot setting "On Click ()" di Inspector lagi!
        if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
        if (_retryButton != null) _retryButton.onClick.AddListener(Retry);
        if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    // ===== FUNGSI UTAMA =====

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;

        _pauseMenuContainer.SetActive(true);
        Time.timeScale = 0f;
        ToggleGameMechanics(false);

        _backgroundOverlay.DOKill();
        _pausePanel.DOKill();

        Sequence pauseSeq = DOTween.Sequence();
        pauseSeq.SetUpdate(true);
        pauseSeq.Join(_backgroundOverlay.DOFade(_targetBgAlpha, _fadeDuration).SetEase(Ease.Linear));
        pauseSeq.Join(_pausePanel.DOScale(Vector3.one, _panelDuration).SetEase(_appearEase));
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;

        _backgroundOverlay.DOKill();
        _pausePanel.DOKill();

        Sequence resumeSeq = DOTween.Sequence();
        resumeSeq.SetUpdate(true);
        resumeSeq.Join(_pausePanel.DOScale(Vector3.zero, _panelDuration).SetEase(_disappearEase));
        resumeSeq.Join(_backgroundOverlay.DOFade(0f, _fadeDuration).SetEase(Ease.Linear).SetDelay(0.1f));

        resumeSeq.OnComplete(() =>
        {
            _pauseMenuContainer.SetActive(false);
            Time.timeScale = 1f;
            ToggleGameMechanics(true);
        });
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        DOTween.KillAll();
        SceneManager.LoadScene("A_MainMenu");
    }

    // ===== HELPER FUNCTIONS =====

    private void ToggleGameMechanics(bool state)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (_playerMovement != null) _playerMovement.enabled = state;
        if (_analogClock != null) _analogClock.enabled = state;
    }
}