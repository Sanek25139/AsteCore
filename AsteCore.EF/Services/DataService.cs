using AsteCore.EF.Model;
using AsteCore.EF.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace AsteCore.EF.Services
{
    public class DataService<TC>(TC context) : IDataService where TC : DbContext
    {
        private readonly TC _context = context;
        private readonly SemaphoreSlim _dbSemaphore = new(1, 1); // Блокировка для всех операций

        public async Task<T> ExecuteDbOperationAsync<T>(Func<TC, Task<T>> operation)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                return await operation(_context);
            }
            
            finally
            {
                _dbSemaphore.Release();
            }
            
        }
        public async Task ExecuteDbOperationAsync(Func<TC, Task> operation)
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                await operation(_context);
            }
            finally
            {
                _dbSemaphore.Release();
            }
        }
        public T ExecuteDbOperation<T>(Func<TC, T> operation)
        {
            _dbSemaphore.Wait(); // Ожидаем семафор синхронно
            try
            {
                return operation(_context); // Выполняем операцию
            }
            finally
            {
                _dbSemaphore.Release(); // Освобождаем семафор
            }
        }


        public IQueryable<T> GetAll<T>() where T : class
        {
            return _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>() where T : class
        {
            return await ExecuteDbOperationAsync(async ctx =>
            {
                var data = await ctx.Set<T>().ToListAsync();
                return data;
            });
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(
            Func<IQueryable<T>, IQueryable<T>>? queryBuilder = null) where T : class
        {
            return await ExecuteDbOperationAsync(async ctx =>
            {
                IQueryable<T> query = ctx.Set<T>();

                if (queryBuilder != null)
                    query = queryBuilder(query);

                return await query.ToListAsync();
            });
        }

        public async Task<T?> GetByIdAsync<T,TID>(TID id) where TID : IEquatable<TID> where T : BaseEntity
        {
            return await ExecuteDbOperationAsync(async ctx =>
                await ctx.Set<T>().FindAsync(id));
        }

        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            return await ExecuteDbOperationAsync(async ctx =>
            {
                await ctx.Set<T>().AddAsync(entity);
                await ctx.SaveChangesAsync();
                return entity;
            });
        }

        public async Task<T> UpdateAsync<T>(T entity) where T : class
        {
            return await ExecuteDbOperationAsync(async ctx =>
            {
                ctx.Entry(entity).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
                return entity;
            });
        }

        public async Task DeleteAsync<T>(T entity) where T : class
        {
            await ExecuteDbOperationAsync(async ctx =>
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
            });
          
        }
        public async Task DeleteRangeAsync<T>(IEnumerable<T> entitis) where T : class
        {
            await ExecuteDbOperationAsync(async ctx =>
            {
                _context.Set<T>().RemoveRange(entitis);
                await _context.SaveChangesAsync();
            });
          
        }

        public async Task SaveChangesAsync()
        {
            await ExecuteDbOperationAsync(async ctx =>
            {
                await _context.SaveChangesAsync();
            });
        }

        public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : class
        {
            await ExecuteDbOperationAsync(async ctx =>
            {
                await _context.Set<T>().AddRangeAsync(entities);
                await _context.SaveChangesAsync();
            });
        }

        public bool HasNavigationProperty<T>(T entity, string navigationPropertyName) where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (string.IsNullOrWhiteSpace(navigationPropertyName)) throw new ArgumentException("Navigation property name cannot be null or empty.", nameof(navigationPropertyName));

            var entry = _context.Entry(entity);
            var navigation = entry.Navigations.FirstOrDefault(n => n.Metadata.Name == navigationPropertyName);

            return navigation != null && navigation.CurrentValue is IEnumerable<object> collection && collection.Any();
        }

        public bool HasNavigationProperty<T>(T entity, string[] navigationPropertyNames) where T : class
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (navigationPropertyNames == null || navigationPropertyNames.Length == 0) throw new ArgumentException("Navigation property names cannot be null or empty.", nameof(navigationPropertyNames));

            var entry = _context.Entry(entity);
            return navigationPropertyNames.Any(npn =>
            {
                var navigation = entry.Navigations.FirstOrDefault(n => n.Metadata.Name == npn);
                // Проверяем, установлена ли связь и не пустая ли коллекция
                return navigation != null && navigation.CurrentValue is IEnumerable<object> collection && collection.Any();
            });
        }

        public async Task<T?> Find<T>(T entity) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            // Проверяем, отслеживается ли объект в контексте
            var localEntity = _context.Set<T>().Local.FirstOrDefault(e => e == entity);
            if (localEntity != null)
            {
                return localEntity; // Возвращаем отслеживаемый объект
            }

            return await _context.Set<T>().FirstOrDefaultAsync(e => e == entity);
        }
        public async Task<T?> First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await ExecuteDbOperationAsync(async ctx =>
            {
                ArgumentNullException.ThrowIfNull(predicate);
                // Запрос к БД (используем Expression для SQL)
                return await ctx.Set<T>().FirstOrDefaultAsync(predicate);
            });
           
        }
        public async Task<T?> First<T>(Func<IQueryable<T>, IQueryable<T>>? queryBuilder, Expression < Func<T, bool>> predicate) where T : class
        {
            return await ExecuteDbOperationAsync(async ctx =>
            {
                ArgumentNullException.ThrowIfNull(predicate);
                // Запрос к БД (используем Expression для SQL)
                if (queryBuilder == null)
                    return await ctx.Set<T>().FirstOrDefaultAsync(predicate);
                else
                {
                    IQueryable<T> query = ctx.Set<T>();
                    query = queryBuilder(query);
                    return await query.FirstOrDefaultAsync(predicate);
                }
            });
                
                
        }

        public EntityEntry<T> GetEntry<T>(T entity) where T : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            return _context.Entry(entity);
        }
    }
}