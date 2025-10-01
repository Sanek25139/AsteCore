
using AsteCore.EF.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace AsteCore.EF.Services.Interface
{
    public interface IDataService
    {
        [Obsolete("Этот метод не защищён семафором и может вызывать race conditions. Используйте GetAllAsync вместо него (и с методом LinqAsync если нужен спецефический запрос).")]
        IQueryable<T> GetAll<T>() where T : class;
        /// <summary>
        /// Возвращает коллекцию сущностей. Для конвертации используйте .ToObservable()/ToList()/ToArray().
        /// Пример: await GetAllAsync<Student>().ToObservable();
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync<T>() where T : class;
        /// <summary>
        /// Возвращает коллекцию сущностей. Для конвертации используйте .ToObservable()/ToList()/ToArray().
        /// Пример: await GetAllAsync<Student>().ToObservable();
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync<T>(Func<IQueryable<T>, IQueryable<T>>? queryBuilder = null) where T : class;


        Task<T?> GetByIdAsync<T,TID>(TID id) where TID : IEquatable<TID> where T : BaseEntity;

        Task<T> AddAsync<T>(T entity) where T : class;
        Task AddRangeAsync<T>(IEnumerable<T> entites) where T : class;

        Task DeleteAsync<T>(T entity) where T : class;
        Task DeleteRangeAsync<T>(IEnumerable<T> entity) where T : class;

        Task<T> UpdateAsync<T>(T entity) where T : class;

        bool HasNavigationProperty<T>(T entity, string navigationPropertyName) where T : class;
        bool HasNavigationProperty<T>(T entity, string[] navigationPropertyNames) where T : class;

        Task<T?> Find<T>(T entity) where T : class;
        Task<T?> First<T>(Expression<Func<T,bool>> predicate) where T : class;
        Task<T?> First<T>(Func<IQueryable<T>, IQueryable<T>>? queryBuilder, Expression<Func<T, bool>> predicate) where T : class;
        EntityEntry<T> GetEntry<T>(T entity) where T : class;




        Task SaveChangesAsync();
    }
}
