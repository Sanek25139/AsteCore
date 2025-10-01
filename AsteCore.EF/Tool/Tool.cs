using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using AsteCore.EF.Model;
using AsteCore.EF.Model.Interface;

namespace AsteCore.EF.Tool
{
    public static class Tool
    {
        public static async Task Reloads<T, TR>(this EntityEntry<T> entity, params Expression<Func<T, IEnumerable<TR>>>[] includes) where T : class where TR : class
        {
            foreach (var include in includes)
            {
                await entity.Collection(include).LoadAsync();
            }
        }



        public static async Task<EntityEntry<T>> TReload<T, TR>(this EntityEntry<T> entity, Expression<Func<T, IEnumerable<TR>>> include) where T : class where TR : class
        {
            await entity.Collection(include).LoadAsync();
            return entity;
        }
        public static async Task<EntityEntry<T>> TReload<T, TR>(this Task<EntityEntry<T>> taskEntity, Expression<Func<T, IEnumerable<TR>>> include) where T : class where TR : class
        {
            var entity = await taskEntity;
            return await entity.TReload(include);
            //return entity;
        }

        public static async Task<EntityEntry<T>> TReload<T, TR>(this EntityEntry<T> entity, Expression<Func<T, TR?>> include) where T : class where TR : class
        {
            await entity.Reference(include).LoadAsync();
            return entity;
        }

        public static async Task<EntityEntry<T>> TReload<T, TR>(this Task<EntityEntry<T>> taskEntity, Expression<Func<T, TR?>> include) where T : class where TR : class
        {
            var entity = await taskEntity;
            return await entity.TReload(include);

        }

        public static async Task<EntityEntry<T>> SetEntry<T, TR>(this Task<EntityEntry<T>> taskEntity, Func<T, TR> property, Func<TR, Task<EntityEntry<TR>>> reload) where T : class where TR : class
        {
            var entity = await taskEntity;

            // Получаем связанное свойство
            var relatedEntity = property(entity.Entity);

            // Если связанное свойство равно null, просто возвращаем текущий entity
            if (relatedEntity == null)
            {
                return entity; // Возвращаем текущий EntityEntry<T>
            }

            // Загружаем связанные данные
            var relatedEntry = entity.Context.Entry(relatedEntity);

            await reload(relatedEntity);

            return entity; // Возвращаем текущий EntityEntry<T>
        }


        public static async Task<ObservableCollection<T>> SearchAsync<T, TC>(this Task<TC> source, Func<T, string> property, string text) where T : BaseEntity where TC : IEnumerable<T>
        {
            IEnumerable<T> records = await source;
            ArgumentNullException.ThrowIfNull(records);
            ArgumentNullException.ThrowIfNull(text);

            var result = records.Where(record =>
            {
                var propertyValue = property(record);
                return propertyValue != null && propertyValue.Contains(text, StringComparison.CurrentCultureIgnoreCase);
            });
            return [.. result];
        }

        public static async Task<ObservableCollection<T>> SearchAsync<T, TC>(this Task<TC> source, string text) where T : IName where TC : IEnumerable<T>
        {
            IEnumerable<T> records = await source;
            ArgumentNullException.ThrowIfNull(records);
            ArgumentNullException.ThrowIfNull(text);

            return [.. records.Where(record => record.Name != null && record.Name.Contains(text, StringComparison.CurrentCultureIgnoreCase))];
        }
        public static async Task<ObservableCollection<T>> SearchByFSMAsync<T, TC>(
        this Task<TC> source,
        string searchText)
        where T : BaseEntity, IFSM
        where TC : IEnumerable<T>
        {
            IEnumerable<T> records = await source;
            ArgumentNullException.ThrowIfNull(records);

            if (string.IsNullOrWhiteSpace(searchText))
                return [.. records];

            return [.. records.Where(r =>
                    r.FSM != null &&
                    r.FSM.Contains(searchText))];
        }

        public static void IsSet<T>(ref T property, T value)
        {
            if (property!.Equals(value))
                return;
            property = value;
        }

        public static async Task<TResult> LinqAsync<T,TResult>(this Task<IQueryable<T>> task ,Func<IEnumerable<T>, TResult> request)
        {
            IQueryable<T> source = await task;
            return request(source);
        }

        //public GetAsyncValue
       
    }
}
