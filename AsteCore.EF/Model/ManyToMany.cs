namespace AsteCore.EF.Model
{
    public class ManyToMany<TLeft, TRight, TID>
    where TID : IEquatable<TID>
    where TLeft : BaseEntity<TID>
    where TRight : BaseEntity<TID>
    {
        public ulong LeftEntityId { get; set; }
        public ulong RightEntityId { get; set; }

        public TLeft? LeftEntity { get; set; }
        public TRight? RightEntity { get; set; }

        public ManyToMany() { }
        public ManyToMany(TLeft? leftEntity, TRight? rightEntity)
        {
            LeftEntity = leftEntity; RightEntity = rightEntity;
        }

        public override bool Equals(object? obj)
        {
            if(obj is ManyToMany<TLeft,TRight,TID> mtm)
            {
                return LeftEntityId == mtm.LeftEntityId && RightEntityId == mtm.RightEntityId;
            }
            else if(obj is TLeft left) 
            {
                return LeftEntity == left;
            }
            else if(obj is TRight right)
            {
                return RightEntity == right;
            }
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(LeftEntityId, RightEntityId);
        }   
    }
}
