using System;
using UnityEngine;

public class TimeService : ITickable
{
    private static DateTime _previousTime = Now;
    private static TimeSpan _debugTimeShift;
    
    private static float _timeScale = 1f;
    private static int _pauseCounter;
    
    public static float DeltaTime { get; private set; }
    
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static DateTime Now => DateTime.UtcNow.Add(_debugTimeShift);
#else
    public static DateTime Now => DateTime.UtcNow;
#endif

    void ITickable.Tick()
    {
        DeltaTime = (float)(Now - _previousTime).TotalSeconds;
        _previousTime = Now;
    }
    
    public static void ShiftTimeBy(TimeSpan time)
    {
        _debugTimeShift += time;
    }
    
    public static void Pause()
    {
        _pauseCounter++;

        if (_pauseCounter > 1)
        {
            return;
        }

        if (Time.timeScale > 0)
        {
            _timeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }
    
    public static void Resume()
    {
        if (_pauseCounter > 0)
        {
            _pauseCounter--;
        }

        if (_pauseCounter > 0)
        {
            return;
        }

        if (Time.timeScale == 0)
        {
            Time.timeScale = _timeScale;
        }
    }
    
    public static void Reset()
    {
        Time.timeScale = 1f;
        _timeScale = 1f;
        _pauseCounter = 0;
    }
}