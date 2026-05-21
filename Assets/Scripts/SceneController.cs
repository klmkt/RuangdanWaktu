using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public bool isFadeIn = false;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup _fadeCanvas;
    [SerializeField] private float _fadeInDuration = 0.4f;
    [SerializeField] private float _fadeOutDuration = 0.4f;
    [SerializeField, Range(0f, 1f)] private float _fadeAlpha = 1f;
    [SerializeField] private float _postFadeDelay = 0f;
    [SerializeField] private bool _ignoreTimeScale = true;

    private bool _isTransitioning = false;

    private void Awake()
    {
        if (_fadeCanvas == null)
        {
            _fadeCanvas = GetComponentInChildren<CanvasGroup>(true);
        }

        if (_fadeCanvas != null)
        {
            _fadeCanvas.alpha = isFadeIn ? _fadeAlpha : 0f;
            _fadeCanvas.blocksRaycasts = false;
            _fadeCanvas.interactable = false;
        }
    }

    void Start()
    {
        if (isFadeIn)
        {
            PlayFadeIn();
        }
    }

    private void PlayFadeIn()
    {
        if (_fadeCanvas == null)
        {
            return;
        }

        _fadeCanvas.gameObject.SetActive(true);
        _fadeCanvas.blocksRaycasts = true;
        _fadeCanvas.interactable = true;

        _fadeCanvas.DOKill();
        Tweener tween = _fadeCanvas.DOFade(0f, _fadeInDuration).SetEase(Ease.Linear);
        if (_ignoreTimeScale)
        {
            tween.SetUpdate(true);
        }

        tween.OnComplete(() =>
        {
            if (_fadeCanvas == null)
            {
                return;
            }

            _fadeCanvas.blocksRaycasts = false;
            _fadeCanvas.interactable = false;
        });
    }

    public void ChangeScene(string sceneName)
    {
        if (_isTransitioning)
        {
            return;
        }

        StartCoroutine(LoadSceneAfterFade(sceneName));
    }

    public void RefreshScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        ChangeScene(currentSceneName);
    }

    private IEnumerator LoadSceneAfterFade(string sceneName = null)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneController: sceneName is not set.");
            yield break;
        }

        _isTransitioning = true;
        Time.timeScale = 0f;

        if (_fadeCanvas != null)
        {
            _fadeCanvas.gameObject.SetActive(true);
            _fadeCanvas.blocksRaycasts = true;
            _fadeCanvas.interactable = true;

            _fadeCanvas.DOKill();
            _fadeCanvas.alpha = 0f;

            Tweener tween = _fadeCanvas.DOFade(_fadeAlpha, _fadeOutDuration).SetEase(Ease.Linear);
            if (_ignoreTimeScale)
            {
                tween.SetUpdate(true);
            }

            if (_fadeOutDuration > 0f)
            {
                yield return tween.WaitForCompletion();
            }
        }

        if (_postFadeDelay > 0f)
        {
            if (_ignoreTimeScale)
            {
                yield return new WaitForSecondsRealtime(_postFadeDelay);
            }
            else
            {
                yield return new WaitForSeconds(_postFadeDelay);
            }
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
