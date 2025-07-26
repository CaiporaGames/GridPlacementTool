using Cysharp.Threading.Tasks;

public interface IUIController
{
    /// <summary>Heavy one-time setup (load assets, data binding).</summary>
    UniTask InitializeAsync();

    /// <summary>Show the screen with optional payload.</summary>
    UniTask ShowAsync(object payload = null);

    /// <summary>Hide the screen.</summary>
    UniTask HideAsync();
}