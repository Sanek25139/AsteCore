namespace AsteCore.MVVM.Interface
{
    public interface ILoadingAsync
    {
        Task LoadAsync();
    }
    public interface ILoadingAsync<T>
    {
        Task LoadAsync(T entity);
    }
    public interface ILoading
    {
        void Load();
    }
    public interface ILoading<T>
    {
        void Load(T entity);
    }

}
