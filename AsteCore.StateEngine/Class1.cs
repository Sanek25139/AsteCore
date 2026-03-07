namespace AsteCore.StateEngine
{
    public abstract class StateEngine<TEnum> where TEnum : struct, Enum
    {
        private readonly Dictionary<Type, StateObject<TEnum>> _objects;

        public StateEngine()
        {
            _objects = [];
        }

        protected abstract void OnModelCreating();
    }

    internal class StateObject<TEnum> where TEnum : struct, Enum
    {
        private TEnum StateMode;

        private readonly Dictionary<string, IEnumerable<TEnum>> _permissions = [];
    }



    public class StateModelBulder()
    {
        internal List<StateObjectBuilder> modelObjectBulders = [];

        public StateObjectBuilder<T> AddObject<T>()
        {
            StateObjectBuilder<T> bulder = new();
            modelObjectBulders.Add(bulder);
            return bulder;
        }
    }

    public abstract class StateObjectBuilder()
    {
        internal abstract Type TypeModel { get; }
        public abstract void Build();
    }

    public class StateObjectBuilder<T>() : StateObjectBuilder()
    {
        internal List<string> actions = [];
        internal override Type TypeModel => typeof(T);

        public override void Build()
        {
            throw new NotImplementedException();
        }
    }


    public 
}
