using System;
using System.Collections.Generic;
using System.Text;

namespace AsteCore.FileEntity
{
    public static class Tool
    {
        public static T GetFreeId<T>(this IEnumerable<T> values) where T : struct, IEquatable<T>, IComparable<T>
        {
            if (values == null || !values.Any())
                return GetDefaultStart<T>();

            var sorted = values.OrderBy(x => x).ToList();
            T expected = GetDefaultStart<T>();

            foreach (var id in sorted)
            {
                if (id.CompareTo(expected) > 0)
                {
                    return expected;
                }
                expected = Increment(expected);
            }

            return expected;
        }

        // Вспомогательные методы
        private static T GetDefaultStart<T>() where T : struct
        {
            // Для числовых типов начинаем с 1
            if (typeof(T) == typeof(int)) return (T)(object)1;
            if (typeof(T) == typeof(long)) return (T)(object)1L;
            if (typeof(T) == typeof(short)) return (T)(object)(short)1;
            if (typeof(T) == typeof(byte)) return (T)(object)(byte)1;

            return default;
        }

        private static T Increment<T>(T value) where T : struct
        {
            // Преобразуем в dynamic для арифметики
            dynamic val = value;

            if (typeof(T) == typeof(int)) return (T)(object)(val + 1);
            if (typeof(T) == typeof(long)) return (T)(object)(val + 1L);
            if (typeof(T) == typeof(short)) return (T)(object)(short)(val + 1);
            if (typeof(T) == typeof(byte)) return (T)(object)(byte)(val + 1);

            throw new NotSupportedException($"Тип {typeof(T).Name} не поддерживает инкремент");
        }

        public static string NormalizeName(string? name, string defaultName)
        {
            if (string.IsNullOrWhiteSpace(name))
                return defaultName;

            return Path.GetFileNameWithoutExtension(name) + ".json";
        }
    }
}
