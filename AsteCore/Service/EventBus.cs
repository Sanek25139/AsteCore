using static AsteCore.Service.CallbackAdapter;

namespace AsteCore.Service
{
    /// <summary>
    /// Реализация шины событий с поддержкой синхронных и асинхронных обработчиков.
    /// <para>Одноразовые подписки (IsSingle)</para>
    /// <para>Автоматическая отписка по таймауту (TimeSpan)</para>
    /// <para>Фильтрация событий (Predicate)</para>
    /// <para>Обработка таймаутов (OnTimeOut)</para>
    /// <para>Отмена отложенных отписок</para>
    /// </summary>
    public class EventBus : IDisposable
    {
        private readonly object _lockObject = new();
        private bool _disposed;
        private readonly Dictionary<Type, List<ICallback>> _signalCallbacks = new();

        private Action<string>? _onOutMessage;

        /// <summary>
        /// Создает экземпляр шины событий с выводом сообщений
        /// </summary>
        /// <param name="outMessage">Делегат для вывода системных сообщений и ошибок</param>
        public EventBus(Action<string> outMessage)
        {
            _onOutMessage = outMessage;
        }
        /// <summary>
        /// Создает экземпляр шины событий без вывода сообщений
        /// </summary>
        public EventBus()
        {
        }
        private EventToken SysSubscribe<T>(ICallback handler)
        {
            Type key = typeof(T);

            lock (_lockObject)
            {
                if (handler.TimeOut.HasValue)
                    DeferredUnsubscription<T>(handler);

                if (!_signalCallbacks.TryGetValue(key, out var list))
                {
                    list = new List<ICallback>();
                    _signalCallbacks[key] = list;
                }

                list.Add(handler);
                list.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            return new EventToken(key, handler, SysUnsubscribe);
        }

        private void SysUnsubscribe(Type key, ICallback handler)
        {
            bool removed = false;
            lock (_lockObject)
            {
                if (_signalCallbacks.TryGetValue(key, out var list))
                {
                    removed = list.Remove(handler);
                    if (list.Count == 0)
                        _signalCallbacks.Remove(key);
                }
            }
            if (removed) 
                handler.Dispose();
        }

        private void SysUnsubscribe<T>(ICallback callback)
        {
            SysUnsubscribe(typeof(T), callback);
        }
        /// <summary>
        /// Отписывает всех подпищиков события типа T
        /// </summary>
        public void AllUnsubscribe<T>()
        {
            Type key = typeof(T);
            lock (_lockObject)
            {
                if (_signalCallbacks.TryGetValue(key, out List<ICallback>? value))
                {
                    foreach (var callback in value)
                    {
                        callback.Dispose();
                    }
                    value.Clear();
                    _signalCallbacks.Remove(key);
                }
            }
        }
        #region Subscribe

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(Func<T, object?, Task> handler, Action<SubscriptionSettings<T>>? configure = null)
        {
            var settings = new SubscriptionSettings<T>();
            configure?.Invoke(settings);

            var callback = settings.Apply(handler);
            return SysSubscribe<T>(callback);
        }

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(Action<T> handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(EventHandler<T> handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(Func<T,Task> handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(AsyncEventHandler<T> handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(Action handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt<T>(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(SenderEvent handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt<T>(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(Func<Task> handler, Action<SubscriptionSettings<T>>? configure = null)
            => Subscribe(Adapt<T>(handler), configure);

        /// <summary>
        /// Подписывает на события типа T с указанным callback-ом и автоматической отпиской по таймауту
        /// </summary>
        /// <typeparam name="T">Тип события, на которое происходит подписка</typeparam>
        /// <param name="handler">Синхронный делегат действия, который будет вызван при наступлении события</param>
        /// <param name="configure">Настройки подписки</param>
        public EventToken Subscribe<T>(AsyncSenderEvent handler, Action<SubscriptionSettings<T>>? configure = null)
           => Subscribe(Adapt<T>(handler), configure);
        #endregion

        /// <summary>
        /// Масивная отписыска от событий
        /// </summary>
        /// <param name="tokens">Токены подписки</param>
        public static void Unsubscribes(IEnumerable<EventToken> tokens) => tokens.Dispose();
        /// <summary>
        /// Отписывается от события
        /// </summary>
        /// <param name="token">Токен подписки</param>
        public static void Unsubscribe(EventToken token) => token.Dispose();

        #region Invoke

        /// <summary>
        /// Вызывает только синхронные обработчики.
        /// Асинхронные обработчики игнорируются!
        /// </summary>
        /// <typeparam name="T">Тип события, которое вызывается</typeparam>
        /// <param name="signal">Экземпляр события для передачи обработчикам</param>
        /// <param name="sender">Отправитель</param>
        public void Invoke<T>(T signal, object? sender = null)
        {
            ICallback[]? callbacks = GetCallbacks<T>();  
            if (callbacks == null) return;

            foreach (var callback in callbacks)
            {
                try
                {
                    callback.InvokeAsync(signal, sender).GetAwaiter().GetResult();
                    if (callback.IsSingle && callback.IsCompleted)
                    {
                        SysUnsubscribe<T>(callback);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    SysUnsubscribe<T>(callback);
                    _onOutMessage?.Invoke($"произошла отписка из-за {ex}");
                }
                
            }
        }
        /// <summary>
        /// Асинхронно вызывает обработчики события типа T.
        /// Выполняет как синхронные, так и асинхронные обработчики.
        /// </summary>
        /// <typeparam name="T">Тип события, которое вызывается</typeparam>
        /// <param name="signal">Экземпляр события для передачи обработчикам</param>
        /// <param name="sender">Отправитель</param>
        public async Task InvokeAsync<T>(T signal, object? sender = null)
        {
            ICallback[]? callbacks = GetCallbacks<T>();
            if (callbacks == null) return;

            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.InvokeAsync(signal, sender);

                    if (callback.IsSingle && callback.IsCompleted)
                    {
                        SysUnsubscribe<T>(callback);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    SysUnsubscribe<T>(callback);
                    _onOutMessage?.Invoke($"произошла отписка из-за {ex}");
                }
            }
        }
        /// <summary>
        /// Вызывает только синхронные обработчики.
        /// Асинхронные обработчики игнорируются!
        /// </summary>
        /// <typeparam name="T">Тип события, которое вызывается</typeparam>
        /// <param name="sender">Отправитель</param>
        public void Invoke<T>(object? sender = null) where T : new()
        {
            ICallback[]? callbacks = GetCallbacks<T>();
            if (callbacks == null) return;

            var signal = new T();
            foreach (var callback in callbacks)
            {
                try
                {
                    callback.InvokeAsync(signal, sender).GetAwaiter().GetResult();

                    if (callback.IsSingle && callback.IsCompleted)
                        SysUnsubscribe<T>(callback);
                }
                catch (ObjectDisposedException ex)
                {
                    SysUnsubscribe<T>(callback);
                    _onOutMessage?.Invoke($"произошла отписка из-за {ex}");
                }
            }
        }
        /// <summary>
        /// Асинхронно вызывает обработчики события типа T.
        /// Создает новый экземпляр события с конструктором по умолчанию.
        /// Выполняет как синхронные, так и асинхронные обработчики.
        /// </summary>
        /// <typeparam name="T">Тип события, которое вызывается (должен иметь конструктор по умолчанию)</typeparam>
        /// <param name="sender">Отправитель</param>
        public async Task InvokeAsync<T>(object? sender = null) where T : new()
        {
            ICallback[]? callbacks = GetCallbacks<T>();
            if (callbacks == null) return;

            var signal = new T();
            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.InvokeAsync(signal, sender);
                    if (callback.IsSingle && callback.IsCompleted)
                        SysUnsubscribe<T>(callback);
                }
                catch (ObjectDisposedException ex)
                {
                    SysUnsubscribe<T>(callback);
                    _onOutMessage?.Invoke($"произошла отписка из-за {ex}");
                }
            }
        }
        #endregion

        private ICallback[]? GetCallbacks<T>()
        {
            Type key = typeof(T);
            ICallback[] callbacksToExecute;

            lock (_lockObject)
            {
                if (!_signalCallbacks.TryGetValue(key, out var callbacks) || callbacks.Count == 0)
                {
                    _onOutMessage?.Invoke("Не найдено подписок для вызова");
                    return null;
                }
                return callbacksToExecute = callbacks.ToArray();
            }
        }

        private void DeferredUnsubscription<T>(ICallback callback)
        {
            if (callback == null || callback.TimeOut == null || callback.Cts == null) return;

            _ = DeferredUnsubscriptionInternalAsync<T>(callback);

        }

        private async Task DeferredUnsubscriptionInternalAsync<T>(ICallback callback)
        {
            Type key = typeof(T);

            try
            {
                await Task.Delay(callback.TimeOut!.Value, callback.Cts!.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            bool wasRemoved = false;

            lock (_lockObject)
            {
                if (_signalCallbacks.TryGetValue(key, out var callbacks))
                {
                    wasRemoved = callbacks.Remove(callback);

                    if (callbacks.Count == 0)
                        _signalCallbacks.Remove(key);
                }
            }

            if (wasRemoved && !callback.IsDisposed)
            {
                try
                {
                    await callback.InvokeOnTimeoutAsync();
                    callback.Dispose();
                }
                catch (Exception ex)
                {

                    _onOutMessage?.Invoke($"{ex} Timeout callback failed");
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if(_disposed) return;

            lock (_lockObject)
            {
                foreach (var callbackList in _signalCallbacks.Values)
                {
                    foreach (var callback in callbackList)
                    {
                        callback.Dispose();
                    }
                    callbackList.Clear();
                }
                _signalCallbacks.Clear();
                _disposed = true;
            }
        }
    }
}
