using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameContainer
{
    private readonly Dictionary<Type, object> _instances = new();
    private readonly List<Type> _bindings = new();
    private readonly HashSet<Type> _bindingsLookup = new();
    private readonly HashSet<Type> _creating = new();

    private readonly List<object> _sceneObjects = new();
    private readonly List<ISavable> _savables = new();

    private readonly GameContainer _parent;
    private readonly GameTicker _gameTicker;

    private bool IsRootContainer => _parent == null;

    public GameContainer(GameTicker gameTicker, GameContainer parent = null)
    {
        _gameTicker = gameTicker;
        _parent = parent;
    }

    public void Bind<T>()
    {
        var type = typeof(T);

        if (_bindingsLookup.Add(type))
        {
            _bindings.Add(type);
        }
    }

    public void BindInstance<T>(T instance)
    {
        var type = typeof(T);

        _instances[type] = instance;

        if (_bindingsLookup.Add(type))
        {
            _bindings.Add(type);
        }
    }

    public void Initialize(Scene scene)
    {
        foreach (var type in _bindings)
        {
            Create(type);
        }

        foreach (var type in _bindings)
        {
            var instance = _instances[type];

            InjectConstruct(instance);
        }

        InjectSceneObjects(scene);

        RegisterSavables();

        if (_savables.Count > 0)
        {
            SaveLoadService.ExecuteLoads(_savables);
        }

        foreach (var type in _bindings)
        {
            var instance = _instances[type];

            if (instance is ITickable tickable)
            {
                _gameTicker.Register(tickable);
            }
        }

        foreach (var sceneObject in _sceneObjects)
        {
            if (sceneObject is ITickable tickable)
            {
                _gameTicker.Register(tickable);
            }
        }

        foreach (var type in _bindings)
        {
            var instance = _instances[type];

            if (instance is IInitializable initializable)
            {
                initializable.Initialize();
            }
        }

        foreach (var sceneObject in _sceneObjects)
        {
            if (sceneObject is IInitializable initializable)
            {
                initializable.Initialize();
            }
        }

        foreach (var type in _bindings)
        {
            if (_instances[type] is IBootstrapper bootstrap)
            {
                bootstrap.Bootstrap();
            }
        }
    }

    public void Dispose()
    {
        for (var i = _bindings.Count - 1; i >= 0; i--)
        {
            var type = _bindings[i];

            if (!_instances.TryGetValue(type, out var instance))
            {
                continue;
            }

            if (instance is ITickable tickable)
            {
                _gameTicker.Unregister(tickable);
            }

            if (instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        foreach (var sceneObject in _sceneObjects)
        {
            if (sceneObject is ITickable tickable)
            {
                _gameTicker.Unregister(tickable);
            }

            if (sceneObject is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        UnregisterSavables();

        if (IsRootContainer)
        {
            SaveLoadService.Dispose();
        }

        _instances.Clear();
        _bindings.Clear();
        _bindingsLookup.Clear();
        _creating.Clear();
        _sceneObjects.Clear();
        _savables.Clear();
    }
    
    public T Instantiate<T>() where T : class
    {
        var type = typeof(T);

        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException($"Cannot instantiate abstract type: {type.Name}");
        }

        var instance = Construct(type);
        
        InjectConstruct(instance);
        
        return (T)instance;
    }
    
    public T Instantiate<T>(T prefab, Transform parent) where T : Component
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab));
        }

        var instance = UnityEngine.Object.Instantiate(prefab, parent);
        var behaviours = instance.gameObject.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var behaviour in behaviours)
        {
            if (behaviour == null || !HasConstructMethod(behaviour))
            {
                continue;
            }

            InjectConstruct(behaviour);
        }
        
        return instance;
    }

    private void RegisterSavables()
    {
        foreach (var type in _bindings)
        {
            var instance = _instances[type];

            if (instance is ISavable savable)
            {
                RegisterSavable(savable);
            }
        }

        foreach (var sceneObject in _sceneObjects)
        {
            if (sceneObject is ISavable savable)
            {
                RegisterSavable(savable);
            }
        }
    }

    private void RegisterSavable(ISavable savable)
    {
        SaveLoadService.Register(savable);
        _savables.Add(savable);
    }

    private void UnregisterSavables()
    {
        foreach (var savable in _savables)
        {
            SaveLoadService.Unregister(savable);
        }

        _savables.Clear();
    }

    private object Get(Type type)
    {
        if (_instances.TryGetValue(type, out var instance))
        {
            return instance;
        }

        if (_parent != null)
        {
            return _parent.Get(type);
        }

        throw new InvalidOperationException($"Type not found: {type.Name}");
    }

    private object Create(Type type)
    {
        if (_instances.TryGetValue(type, out var instance))
        {
            return instance;
        }

        if (type.IsAbstract || type.IsInterface)
        {
            throw new InvalidOperationException($"Cannot instantiate abstract type: {type.Name}");
        }

        var created = Construct(type);
        _instances[type] = created;
        return created;
    }

    private object Construct(Type type)
    {
        if (!_creating.Add(type))
        {
            throw new InvalidOperationException($"Circular dependency detected: {type.Name}");
        }

        try
        {
            var constructor = GetConstructor(type);
            var parameters = constructor.GetParameters();
            var dependencies = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                dependencies[i] = ResolveParameter(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(type, dependencies);
        }
        finally
        {
            _creating.Remove(type);
        }
    }

    private ConstructorInfo GetConstructor(Type type)
    {
        var constructors = type.GetConstructors();

        if (constructors.Length == 0)
        {
            throw new InvalidOperationException($"No public constructor found: {type.Name}");
        }

        var selected = constructors[0];
        var maxParameters = selected.GetParameters().Length;

        for (var i = 1; i < constructors.Length; i++)
        {
            var current = constructors[i];
            var count = current.GetParameters().Length;

            if (count > maxParameters)
            {
                selected = current;
                maxParameters = count;
            }
        }

        return selected;
    }

    private object ResolveParameter(Type type)
    {
        if (type == typeof(GameContainer))
        {
            return this;
        }
        
        if (_instances.TryGetValue(type, out var instance))
        {
            return instance;
        }

        if (_bindingsLookup.Contains(type))
        {
            return Create(type);
        }

        if (_parent != null)
        {
            return _parent.Get(type);
        }

        throw new InvalidOperationException($"Dependency not found: {type.Name}");
    }

    private void InjectSceneObjects(Scene scene)
    {
        var rootObjects = scene.GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                var type = behaviour.GetType();

                if (TryGet(type) != null)
                {
                    continue;
                }

                var hasConstruct = HasConstructMethod(behaviour);
                var isSavable = typeof(ISavable).IsAssignableFrom(type);
                var isInitializable = typeof(IInitializable).IsAssignableFrom(type);
                var isTickable = typeof(ITickable).IsAssignableFrom(type);
                var isDisposable = typeof(IDisposable).IsAssignableFrom(type);
                
                if (!hasConstruct && !isSavable && !isInitializable && !isTickable && !isDisposable)
                {
                    continue;
                }

                _sceneObjects.Add(behaviour);

                if (hasConstruct)
                {
                    InjectConstruct(behaviour);
                }
            }
        }
    }

    private void InjectConstruct(object instance)
    {
        var methods = instance
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        MethodInfo constructMethod = null;

        foreach (var method in methods)
        {
            if (Attribute.IsDefined(method, typeof(InjectAttribute)))
            {
                constructMethod = method;
                break;
            }
        }

        if (constructMethod == null)
        {
            return;
        }

        var parameters = constructMethod.GetParameters();
        var dependencies = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            dependencies[i] = ResolveParameter(parameters[i].ParameterType);
        }

        constructMethod.Invoke(instance, dependencies);
    }

    private bool HasConstructMethod(object instance)
    {
        var methods = instance
            .GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            if (Attribute.IsDefined(method, typeof(InjectAttribute)))
            {
                return true;
            }
        }

        return false;
    }

    private object TryGet(Type type)
    {
        if (_instances.TryGetValue(type, out var instance))
        {
            return instance;
        }

        return _parent?.TryGet(type);
    }
}