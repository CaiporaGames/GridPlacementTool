using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Base class for MonoBehaviour-based screens
/// </summary>
public abstract class BaseUIController : MonoBehaviour, IUIController
{
    [SerializeField] private CanvasGroup _canvasGroup;

    protected virtual void Awake()
    {
        gameObject.SetActive(false);
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual async UniTask InitializeAsync()
    {
        // Override for loading assets or bindings
        await UniTask.Yield();
    }

    public async UniTask ShowAsync(object payload = null)
    {
        gameObject.SetActive(true);
        await Fade(0, 1);
        OnShow(payload);
    }

    public async UniTask HideAsync()
    {
        OnHide();
        await Fade(1, 0);
        gameObject.SetActive(false);
    }

    protected virtual void OnShow(object payload) { }
    protected virtual void OnHide() { }

    private async UniTask Fade(float from, float to)
    {
        float t = 0f;
        const float duration = 0.25f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            await UniTask.Yield();
        }
        _canvasGroup.alpha = to;
    }
}