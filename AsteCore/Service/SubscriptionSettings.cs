using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsteCore.Service
{
    /// <summary>
    /// Настройки подписки
    /// </summary>
    /// <typeparam name="T">тип события</typeparam>
    public class SubscriptionSettings<T>
    {
        private bool _isSingle;
        private int _priority = 10;
        private TimeSpan? _timeout;

        private Func<T, bool>? _predicate;
        private Func<Task>? _onTimeout;


        internal Callback<T> Apply(Func<T, object?, Task> handler)
        {
            return new Callback<T>(
                handler,
                isSingle: _isSingle,
                timeOut: _timeout,
                predicate: _predicate,
                onTimeout: _onTimeout,
                priority: _priority
            );
        }
        /// <summary>
        /// Одноразовая подписка
        /// </summary>
        public SubscriptionSettings<T> AsSingle()
        {
            _isSingle = true;
            return this;
        }

        /// <summary>
        /// Таймаут с асинхронным обработчиком
        /// </summary>
        public SubscriptionSettings<T> WithTimeout(
            TimeSpan timeout,
            Func<Task>? onTimeout = null)
        {
            EnsureTimeoutNotSet();
            _timeout = timeout;
            _onTimeout = onTimeout;
            return this;
        }

        /// <summary>
        /// Таймаут с синхронным обработчиком
        /// </summary>
        public SubscriptionSettings<T> WithTimeout(
            TimeSpan timeout,
            Action? onTimeout)
        {
            EnsureTimeoutNotSet();
            _timeout = timeout;

            if (onTimeout != null)
                _onTimeout = () =>
                {
                    onTimeout();
                    return Task.CompletedTask;
                };

            return this;
        }

        /// <summary>
        /// Условие вызова
        /// </summary>
        public SubscriptionSettings<T> WithPredicate(Func<T, bool> predicate)
        {
            _predicate ??= predicate
                ?? throw new ArgumentNullException(nameof(predicate));

            return this;
        }

        /// <summary>
        /// Приоритет
        /// </summary>
        public SubscriptionSettings<T> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        private void EnsureTimeoutNotSet()
        {
            if (_timeout != null)
                throw new InvalidOperationException(
                    "Timeout можно задать только один раз");
        }
    }
}

