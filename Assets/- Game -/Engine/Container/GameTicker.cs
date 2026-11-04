using System.Collections.Generic;

public class GameTicker
{
    private readonly List<ITickable> _tickableCollection = new();

    public void Register(ITickable tickable)
    {
        if (_tickableCollection.Contains(tickable))
        {
            return;
        }

        _tickableCollection.Add(tickable);
    }

    public void Unregister(ITickable tickable)
    {
        _tickableCollection.Remove(tickable);
    }

    public void Tick()
    {
        foreach (var tickable in _tickableCollection)
        {
            tickable.Tick();
        }
    }
}