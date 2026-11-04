using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-9999)]
public class CoreContext : MonoBehaviour
{
    [SerializeField] private List<BaseInstaller> _installers;
    
    public static GameTicker GameTicker { get; private set; }
    public static GameContainer Container { get; private set; }
    public static bool IsReady { get; private set; }
    
    private static CoreContext Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        GameTicker = new GameTicker();
        Container = new GameContainer(GameTicker);

        foreach (var installer in _installers)
        {
            installer.Install(Container);
        }

        Container.Initialize(gameObject.scene);
        IsReady = true;
    }
    
    private void Update()
    {
        GameTicker?.Tick();
    }

    public static void Shutdown()
    {
        SaveLoadService.ExecuteSaves();
        
        if (Instance == null)
        {
            return;
        }

        Destroy(Instance.gameObject);
    }
    
    private void OnDestroy()
    {
        SaveLoadService.ExecuteSaves();
        
        Container?.Dispose();
        Container = null;
        GameTicker = null;
        IsReady = false;
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    private void OnApplicationPause(bool status)
    {
        SaveLoadService.ExecuteSaves();
    }
    
    private void OnApplicationFocus(bool status)
    {
        SaveLoadService.ExecuteSaves();
    }

    private void OnApplicationQuit()
    {
        SaveLoadService.ExecuteSaves();
    }
}
