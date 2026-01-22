namespace AsteCore.FileEntity.Interfaces
{
    internal interface IChangeTracking
    {
        bool IsDirty { get; set; }
        void AcceptChanges();
        void MarkDirty();
    }
}
