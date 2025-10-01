using AsteCore.EF.Model.Interface;

namespace AsteCore.EF.Model
{
    public class NameEntity<TID> : BaseEntity<TID>, IName where TID : IEquatable<TID> 
    {
        public string Name { get; set; } = null!;

        public override string ToString()
        {
            return Name;
        }
    }

    public class NameEntityInt : NameEntity<int> { }
    public class NameEntityUlong : NameEntity<ulong> { }
}
