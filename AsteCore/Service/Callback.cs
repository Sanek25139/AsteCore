namespace AsteCore.Service
{
    /// <summary>
    /// Асихроный вариант EventHandler
    /// </summary>
    /// <typeparam name="TEventArgs">Тип сигнала</typeparam>
    /// <param name="sender">Отправитель</param>
    /// <param name="e">Сигнал</param>
    /// <returns></returns>
    public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
    /// <summary>
    /// событие с отправителем
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <returns></returns>
    public delegate void SenderEvent(object? sender);
    /// <summary>
    /// асихроное событие с отправителем
    /// </summary>
    /// <param name="sender">Отправитель</param>
    /// <returns></returns>
    public delegate Task AsyncSenderEvent(object? sender);
    internal class Callback<T> : ICallback
    {
        private readonly WeakReference? _target;

        private Func<T, object?, Task>? _handler;
        private Func<T, bool>? _predicate;
        private Func<Task>? _onTimeout;

        private bool _disposed;

        public bool IsDisposed => _disposed;
        public bool IsSingle { get; }
        public bool IsCompleted { get; private set; }
        public int Priority { get; }

        public TimeSpan? TimeOut { get; }
        public CancellationTokenSource? Cts { get; }

        public Callback(
         Func<T, object?, Task> handler,
         bool isSingle = false,
         TimeSpan? timeOut = null,
         Func<T, bool>? predicate = null,
         Func<Task>? onTimeout = null,
         int priority = 0)
        {
            _handler = handler;

            if (handler.Target != null)
                _target = new WeakReference(handler.Target);

            _predicate = predicate;
            _onTimeout = onTimeout;

            IsSingle = isSingle;
            Priority = priority;
            TimeOut = timeOut;

            if (timeOut != null)
                Cts = new CancellationTokenSource();
        }
        public async Task InvokeAsync(object? signal, object? sender)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(Callback<T>));

            if (signal is not T typed)
                return;

            if (_predicate != null && !_predicate(typed))
                return;
            if(_handler == null)
                throw new ObjectDisposedException("Handler null");
            if (_target != null && !_target.IsAlive)
                throw new ObjectDisposedException("Target collected");


            await _handler.Invoke(typed, sender).ConfigureAwait(false);
            IsCompleted = true;
        }
        public async Task InvokeOnTimeoutAsync()
        {
            if (_disposed || IsCompleted || _onTimeout == null)
                return;

            if (_target != null && !_target.IsAlive)
                throw new ObjectDisposedException("Target collected");

            await _onTimeout().ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Cts?.Cancel();
            Cts?.Dispose();
            _handler = null;
            _predicate = null;
            _onTimeout = null;
        }
    }

    internal interface ICallback : IDisposable
    {
        bool IsDisposed { get; }
        bool IsSingle { get; }
        bool IsCompleted { get; }
        int Priority { get; }

        TimeSpan? TimeOut { get; }
        CancellationTokenSource? Cts { get; }

        // Универсальный вызов без знания T
        Task InvokeAsync(object? signal, object? sender);

        // Таймаут
        Task InvokeOnTimeoutAsync();
    }
}

