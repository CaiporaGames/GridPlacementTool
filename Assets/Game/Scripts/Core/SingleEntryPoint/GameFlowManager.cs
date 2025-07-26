using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class GameFlowManager : MonoBehaviour
{
    /*  [SerializeField] private PlayerChunkStreamer playerChunkhunkStreamer = null;
     private PlayerSpawner playerSpawner = null;
  */
    [SerializeField] private UIService _uiService;
    [SerializeField] private MainMenuController _mainMenu;
    [SerializeField] private BinarySaveService _saveService;
    [SerializeField] private MonoPool<GameObject> _bulletPool;
    [SerializeField] UpdateManager _updateManager;
    private List<IGameSystem> systems = new();

    private void Awake()
    {
        ServiceLocator.RegisterSingleton<IUIService>(_uiService);
        ServiceLocator.RegisterSingleton<ISaveService>(_saveService);
        ServiceLocator.RegisterSingleton<IObjectPool<GameObject>>(_bulletPool);
        ServiceLocator.RegisterSingleton<IUpdateManager>(_updateManager);
        
        _uiService.Register(ScreenTypes.MainMenu, _mainMenu);
        // _uiService.Register("Settings", _settingsController);
    }
    public async void Start()
    {
        var ui = ServiceLocator.Resolve<IUIService>();
        var save  = ServiceLocator.Resolve<ISaveService>();
        var pool  = ServiceLocator.Resolve<IObjectPool<GameObject>>();

        // Show main menu at game start
        await ui.ShowScreenAsync(ScreenTypes.MainMenu);
        _uiService.Register(ScreenTypes.MainMenu, _mainMenu);
        /*  playerSpawner = FindFirstObjectByType<PlayerSpawner>();
          playerSpawner.Initialize(transform.GetComponent<PlayerChunkStreamer>());
          GetComponent<HitDetector>().cam = playerSpawner.GetCamera;
          systems.Add(playerChunkhunkStreamer);
          systems.Add(playerSpawner);

          // at this point every peer finished loading GameScene
          RunGameFlowAsync().Forget(); */
    }

    private async UniTask RunGameFlowAsync()
    {
        // PHASE 1 – heavy boot
      /*   foreach (var sys in systems)
            await sys.InitializeAsync();

        await UniTask.Delay(500);
        //Spawn Player and it prefabs
        await playerSpawner.ActivatePlayer(); */

        /*   uiLoadingSystem.ShowLoadingScreen();

                  foreach (var system in systems)
                  {
                      await system.InitializeAsync();
                  }

                  await gridSystem.CreateGridAsync();
                  uiLoadingSystem.SetProgress(0.25f);

                  await playersManager.SpawnPlayers();
                  uiLoadingSystem.SetProgress(0.50f);

                  await dailyTasksController.UpdateDailyTasks();
                  uiLoadingSystem.SetProgress(0.75f);

                  await shopController.UpdateShop();
                  uiLoadingSystem.SetProgress(1f);

                  await UniTask.Delay(500);
                  uiLoadingSystem.HideLoadingScreen(); */
    }
}