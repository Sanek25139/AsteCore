namespace AsteCore.Service
{
    /// <summary>
    /// Токен подписки
    /// </summary>
    public sealed class EventToken : IDisposable
    {
        private readonly ICallback _callback;
        private readonly Type _key;
        private readonly Action<Type, ICallback> _unsubscribe;
        private bool _disposed;

        internal EventToken(Type key, ICallback callback, Action<Type, ICallback> unsubscribe)
        {
            _key = key;
            _callback = callback;
            _unsubscribe = unsubscribe;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _unsubscribe(_key, _callback);
        }
    }
}
