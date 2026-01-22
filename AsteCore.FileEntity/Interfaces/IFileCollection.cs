namespace AsteCore.FileEntity.Interfaces
{
    public interface IFileCollection<T> : ICollection<T>
    {
        IFileObject<T>? GetObject(Func<T,bool> predicate);
        IEnumerable<IFileObject<T>> Objects { get; }
        new IFileObject<T>? Add(T item);
    }
}
