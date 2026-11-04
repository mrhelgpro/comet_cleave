using System;
using System.Collections.Generic;
using UnityEngine;

public class ScheduleService : ITickable, IDisposable
{
    private static readonly List<TimerEntry> _timerCollection = new();
    private static readonly List<NextFrameEntry> _nextFrameCallbackCollection = new();
    private static readonly List<NextFrameEntry> _nextFrameCallbackBuffer = new();
    private static readonly List<TimerEntry> _dueCallbackCollection = new();

    private static long _nextIdentifier = 1;
    
    void ITickable.Tick()
    {
        if (_nextFrameCallbackCollection.Count > 0)
        {
            _nextFrameCallbackBuffer.AddRange(_nextFrameCallbackCollection);
            _nextFrameCallbackCollection.Clear();

            for (var index = 0; index < _nextFrameCallbackBuffer.Count; index++)
            {
                var callback = _nextFrameCallbackBuffer[index].Callback;
                callback?.Invoke();
            }

            _nextFrameCallbackBuffer.Clear();
        }

        for (var index = 0; index < _timerCollection.Count; index++)
        {
            var timerEntry = _timerCollection[index];
            var deltaTime = timerEntry.IgnoreTimeScale 
                ? Time.unscaledDeltaTime 
                : Time.deltaTime;
            
            timerEntry.Remaining -= deltaTime;
            _timerCollection[index] = timerEntry;

            if (timerEntry.Remaining > 0f)
            {
                continue;
            }

            if (timerEntry.Callback != null)
            {
                _dueCallbackCollection.Add(timerEntry);
            }
        }

        for (var index = _timerCollection.Count - 1; index >= 0; index--)
        {
            if (_timerCollection[index].Remaining <= 0f)
            {
                _timerCollection.RemoveAt(index);
            }
        }

        for (var index = 0; index < _dueCallbackCollection.Count; index++)
        {
            _dueCallbackCollection[index].Callback?.Invoke();
        }

        _dueCallbackCollection.Clear();
    }
    
    void IDisposable.Dispose()
    {
        _timerCollection.Clear();
        _nextFrameCallbackCollection.Clear();
        _nextFrameCallbackBuffer.Clear();
        _dueCallbackCollection.Clear();
    }

    public static void RunNextFrame(Action callback)
    {
        var identifier = _nextIdentifier++;
        
        _nextFrameCallbackCollection.Add(new NextFrameEntry
        {
            Identifier = identifier,
            Callback = callback
        });
    }
    
    public static ScheduleHandle RunAfter(float duration, Action callback, bool ignoreTimeScale = false)
    {
        if (duration <= 0f)
        {
            callback?.Invoke();
            return null;
        }

        var identifier = _nextIdentifier++;
        
        _timerCollection.Add(new TimerEntry
        {
            Identifier = identifier,
            Remaining = duration,
            IgnoreTimeScale = ignoreTimeScale,
            Callback = callback
        });

        return new ScheduleHandle(identifier);
    }

    internal static void Cancel(long identifier)
    {
        for (var index = _timerCollection.Count - 1; index >= 0; index--)
        {
            if (_timerCollection[index].Identifier == identifier)
            {
                _timerCollection.RemoveAt(index);
                return;
            }
        }

        for (var index = _nextFrameCallbackCollection.Count - 1; index >= 0; index--)
        {
            if (_nextFrameCallbackCollection[index].Identifier == identifier)
            {
                _nextFrameCallbackCollection.RemoveAt(index);
                return;
            }
        }

        for (var index = _nextFrameCallbackBuffer.Count - 1; index >= 0; index--)
        {
            if (_nextFrameCallbackBuffer[index].Identifier == identifier)
            {
                _nextFrameCallbackBuffer[index] = new NextFrameEntry
                {
                    Identifier = identifier,
                    Callback = null
                };
                return;
            }
        }

        for (var index = 0; index < _dueCallbackCollection.Count; index++)
        {
            if (_dueCallbackCollection[index].Identifier != identifier)
            {
                continue;
            }

            var timerEntry = _dueCallbackCollection[index];
            timerEntry.Callback = null;
            _dueCallbackCollection[index] = timerEntry;
            return;
        }
    }

    private struct TimerEntry
    {
        public long Identifier;
        public float Remaining;
        public bool IgnoreTimeScale;
        public Action Callback;
    }

    private struct NextFrameEntry
    {
        public long Identifier;
        public Action Callback;
    }
}
