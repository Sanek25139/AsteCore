namespace AsteCore.EF.Model.Interface
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }

        void Delete()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
