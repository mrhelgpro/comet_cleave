using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public static class SceneLoadService
{
    public static event Action OnLoading;
    public static event Action OnLoaded;
    
    private const string CORE_SCENE = "CoreScene";
    private const string GAMEPLAY_SCENE = "GameplayScene";

    public static bool ShouldRestoreScene { get; private set; }

    private static string _sceneNameToRestore;

    public static bool IsCoreScene(this Scene scene)
    {
        return scene.name == CORE_SCENE;
    }

    public static void LoadCoreScene()
    {
        _ = LoadScene(CORE_SCENE);
    }

    public static void LoadGameplayScene(bool canReload = false)
    {
        _ = LoadScene(GAMEPLAY_SCENE, canReload);
    }
    
    public static void ReloadContext(string currentScene)
    {
        _sceneNameToRestore = currentScene;
        ShouldRestoreScene = true;
        
        _ = LoadScene(CORE_SCENE);
    }

    public static void RestoreScene()
    {
        ShouldRestoreScene = false;
        
        _ = LoadScene(_sceneNameToRestore);
    }
    
    private static async Task LoadScene(string sceneName, bool canReload = false)
    {
        if (canReload || SceneManager.GetActiveScene().name != sceneName)
        {
            OnLoading?.Invoke();
            var loadOperation = SceneManager.LoadSceneAsync(sceneName);

            while (loadOperation is { isDone: false })
            {
                await Task.Yield();
            }

            OnLoaded?.Invoke();
        }
    }
}