using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AsteCore.EF.Model
{
    public class BaseEntity
    {

    }

    public class BaseEntity<TID> : BaseEntity where TID : IEquatable<TID>
    {
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        public TID Id { get; set; }
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Рассмотрите возможность добавления модификатора "required" или объявления значения, допускающего значение NULL.
        
        public override bool Equals(object? obj)
        {
            if (obj is BaseEntity<TID> entity)
            {
                return Id.Equals(entity.Id); // Теперь можно использовать Equals
            }
            return false;
        }

        // Переопределяем метод GetHashCode
        public override int GetHashCode()
        {
            return Id.GetHashCode(); // Возвращаем хэш-код Id
        }

        // Перегружаем оператор ==
        public static bool operator ==(BaseEntity<TID>? left, BaseEntity<TID>? right)
        {
            // Проверяем на null и сравниваем
            if (ReferenceEquals(left, right)) return true; // Оба null или оба указывают на один объект
            if (left is null || right is null) return false; // Один из них null
            return left.Equals(right); // Сравниваем объекты
        }

        public static bool operator !=(BaseEntity<TID>? left, BaseEntity<TID>? right)
        {
            return !(left == right); // Используем перегруженный оператор ==
        }

        public static bool operator ==(BaseEntity<TID> entity, ulong id)
        {
            // Проверка на null
            if (entity is null)
            {
                return false; // Если entity null, то оно не равно id
            }
            return entity.Equals(id); // Сравнение Id с переданным значением
        }

        // Перегрузка оператора !=
        public static bool operator !=(BaseEntity<TID> entity, ulong id)
        {
            return !(entity == id); // Используем перегруженный оператор ==
        }

        public static implicit operator TID(BaseEntity<TID> entity)
        {
            // Проверка на null
            if (entity is null)
            {
                throw new InvalidOperationException("Cannot convert null BaseEntity to ulong.");
            }
            return entity.Id; // Возвращаем Id
        }
    }

    public class BaseEntityInt : BaseEntity<int> { }
    public class BaseEntityUlong : BaseEntity<ulong> { }
}
