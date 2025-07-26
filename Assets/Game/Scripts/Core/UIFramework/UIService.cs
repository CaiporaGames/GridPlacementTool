using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages lifecycle of all UI controllers
/// </summary>
public class UIService : MonoBehaviour, IUIService
{
    private readonly Dictionary<ScreenTypes, IUIController> _controllers = new();
    private readonly HashSet<ScreenTypes> _initialized = new();

    public void Register(ScreenTypes key, IUIController controller)
    {
        if (_controllers.ContainsKey(key))
            throw new ArgumentException($"Controller with key '{key}' already registered.");

        _controllers[key] = controller;
    }

    public async UniTask ShowScreenAsync(ScreenTypes key, object payload = null)
    {
        if (!_controllers.TryGetValue(key, out var ctrl))
            throw new KeyNotFoundException($"No UI controller for key '{key}'.");

        if (!_initialized.Contains(key))
        {
            await ctrl.InitializeAsync();
            _initialized.Add(key);
        }

        await ctrl.ShowAsync(payload);
    }

    public UniTask HideScreenAsync(ScreenTypes key)
        => _controllers.TryGetValue(key, out var ctrl)
            ? ctrl.HideAsync()
            : throw new KeyNotFoundException($"No UI controller for key '{key}'.");
}