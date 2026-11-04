using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-999)]
public class SceneContext : MonoBehaviour
{
    [SerializeField] private List<BaseInstaller> _installers;
        
    private GameContainer _container;

    private static SceneContext Instance { get; set; }
        
    private void Awake()
    {
        Instance = this;

        if (CoreContext.IsReady)
        {
            BuildContainer();
            return;
        }

        SceneLoadService.ReloadContext(gameObject.scene.name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (_container == null)
        {
            return;
        }

        SaveLoadService.ExecuteSaves();
        _container.Dispose();
        _container = null;
    }

    private void BuildContainer()
    {
        _container = new GameContainer(CoreContext.GameTicker, CoreContext.Container);

        foreach (var installer in _installers)
        {
            installer.Install(_container);
        }

        _container.Initialize(gameObject.scene);
    }
}
