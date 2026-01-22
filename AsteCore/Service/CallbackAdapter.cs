namespace AsteCore.Service
{
    internal static class CallbackAdapter
    {
        public static Func<T, object?, Task> Adapt<T>(Action<T> handler)
       => (e, _) =>
       {
           handler(e);
           return Task.CompletedTask;
       };

        public static Func<T, object?, Task> Adapt<T>(EventHandler<T> handler)
            => (e, sender) =>
            {
                handler(sender, e);
                return Task.CompletedTask;
            };

        public static Func<T, object?, Task> Adapt<T>(Func<T, Task> handler)
            => (e, _) => handler(e);

        public static Func<T, object?, Task> Adapt<T>(AsyncEventHandler<T> handler)
            => (e, sender) => handler(sender, e);

        public static Func<T, object?, Task> Adapt<T>(Action handler)
            => (_, _) =>
            {
                handler();
                return Task.CompletedTask;
            };

        public static Func<T, object?, Task> Adapt<T>(Func<Task> handler)
            => (_, _) => handler();

        public static Func<T, object?, Task> Adapt<T>(SenderEvent handler)
            => (_, sender) =>
            {
                handler(sender);
                return Task.CompletedTask;
            };

        public static Func<T, object?, Task> Adapt<T>(AsyncSenderEvent handler)
            => (_, sender) => handler(sender);
    }
}
