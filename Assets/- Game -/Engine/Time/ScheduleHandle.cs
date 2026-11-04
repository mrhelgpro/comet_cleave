using System;

public sealed class ScheduleHandle : IDisposable
{
    private long _identifier;
    private bool _isDisposed;

    internal ScheduleHandle(long identifier)
    {
        _identifier = identifier;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        ScheduleService.Cancel(_identifier);
    }
}
