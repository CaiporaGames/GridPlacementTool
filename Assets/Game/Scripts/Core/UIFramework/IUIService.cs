using Cysharp.Threading.Tasks;

public interface IUIService
{
    /// <summary>Registers a controller under a key.</summary>
    void Register(ScreenTypes key, IUIController controller);

    /// <summary>Show a registered screen by key.</summary>
    UniTask ShowScreenAsync(ScreenTypes key, object payload = null);

    /// <summary>Hide a registered screen by key.</summary>
    UniTask HideScreenAsync(ScreenTypes key);
}
