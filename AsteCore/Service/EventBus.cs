using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;


namespace StarterKit.WPF.Service
{
    public class EventBus
    {
        private readonly Dictionary<string, List<Delegate>> _signalCallbacks = new();

        private Action<string>? _onOutMessage;
        public EventBus(Action<string> outMessage)
        {
            _onOutMessage = outMessage;
        }

        public void Subscribe<T>(Action<T> callback)
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                _signalCallbacks[key].Add(callback);
            }
            else
            {
                _signalCallbacks.Add(key, new() { callback });
            }
        }
        public void Subscribe<T>(Func<T,Task> callback)
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                _signalCallbacks[key].Add(callback);
            }
            else
            {
                _signalCallbacks.Add(key, new() { callback });
            }
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                _signalCallbacks[key].Remove(callback);
            }
            else
            {
                _onOutMessage?.Invoke("не существует такого сигнала для отписки");
            }
        }
        public void Unsubscribe<T>(Func<T, Task> callback)
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                _signalCallbacks[key].Remove(callback);
            }
            else
            {
                _onOutMessage?.Invoke("не существует такого сигнала для отписки");
            }
        }
        /// <summary>
        /// Вызывает только синхронные обработчики.
        /// Асинхронные обработчики игнорируются!
        /// </summary>
        public void Invoke<T>(T signal)
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                foreach (var obj in _signalCallbacks[key].ToList())
                {
                    if (obj is Action<T> callback)
                        callback?.Invoke(signal);
                }
            }
            else
            {
                _onOutMessage?.Invoke("не существует такого сигнала для возыва");
            }
        }
        public async Task InvokeAsync<T>(T signal)
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                foreach (var obj in _signalCallbacks[key].ToList())
                {
                    if (obj is Func<T, Task> asyncCallback)
                    {
                        await asyncCallback(signal);
                    }
                    else if (obj is Action<T> syncCallback)
                    {
                        syncCallback(signal); 
                    }
                }
            }
            else
            {
                _onOutMessage?.Invoke("не существует такого сигнала для возыва");
            }
        }
        /// <summary>
        /// Вызывает только синхронные обработчики.
        /// Асинхронные обработчики игнорируются!
        /// </summary>
        public void Invoke<T>() where T : new()
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                foreach (var obj in _signalCallbacks[key].ToList())
                {
                    if (obj is Action<T> callback)
                        callback?.Invoke(new());
                }
            }
            else
            {
                _onOutMessage?.Invoke("не существует такого сигнала для возыва");
            }
        }
        public async Task InvokeAsync<T>() where T : new()
        {
            string key = typeof(T).Name;
            if (_signalCallbacks.ContainsKey(key))
            {
                foreach (var obj in _signalCallbacks[key].ToList())
                {
                    if (obj is Func<T, Task> asyncCallback)
                    {
                        await asyncCallback(new()); 
                    }
                    else if (obj is Action<T> syncCallback)
                    {
                        syncCallback(new()); 
                    }
                }
            }
            else
            {
                _onOutMessage?.Invoke("не существует такого сигнала для возыва");
            }
        }

    }
    
}
