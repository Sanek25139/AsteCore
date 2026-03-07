namespace AsteCore.FileEntity.Interfaces
{
    public interface IFileObject<T>
    {
        T? Value { get; set; }
        void Update(Action<T> onUpdate);
    }
}
