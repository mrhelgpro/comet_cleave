using System;
using System.Collections.Generic;

public static class SaveLoadService
{
    private static readonly List<ISavable> _allSavables = new();
    private static readonly HashSet<ISavable> _pendingSavables = new();
    private static readonly HashSet<ISavable> _registeredSavables = new();
    private static readonly Dictionary<ISavable, Action> _onChangedHandlers = new();
    
    public static void Register(ISavable savable)
    {
        if (!_registeredSavables.Add(savable))
        {
            return;
        }

        _allSavables.Add(savable);

        Action handler = () => _pendingSavables.Add(savable);
        _onChangedHandlers[savable] = handler;
        savable.OnSaveRequired += handler;
    }

    public static void Unregister(ISavable savable)
    {
        if (!_registeredSavables.Remove(savable))
        {
            return;
        }

        _allSavables.Remove(savable);
        _pendingSavables.Remove(savable);

        if (_onChangedHandlers.TryGetValue(savable, out var handler))
        {
            savable.OnSaveRequired -= handler;
            _onChangedHandlers.Remove(savable);
        }
    }

    public static void ExecuteLoads()
    {
        ExecuteLoads(_allSavables);
    }

    public static void ExecuteLoads(IReadOnlyList<ISavable> savables)
    {
        foreach (var savable in savables)
        {
            savable.Load();
        }
    }

    public static void ExecuteSaves()
    {
        foreach (var savable in _pendingSavables)
        {
            savable.Save();
        }

        _pendingSavables.Clear();
    }
    
    public static void Dispose()
    {
        foreach (var savable in _allSavables.ToArray())
        {
            Unregister(savable);
        }

        _allSavables.Clear();
        _pendingSavables.Clear();
        _registeredSavables.Clear();
        _onChangedHandlers.Clear();
    }
}