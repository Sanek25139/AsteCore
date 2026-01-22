using System.ComponentModel;
using System.Reflection;

namespace AsteCore
{
    /// <summary>
    /// Набор методов-инструментов
    /// </summary>
    public static class Tool
    {
        /// <summary>
        /// Расширение для вызова Dispose для всех элементов коллекции, реализующих IDisposable.
        /// Гарантирует обработку каждого элемента даже при возникновении исключений в одном из них.
        /// </summary>
        /// <param name="objs">Коллекция объектов для освобождения</param>
        /// <exception cref="AggregateException">Может содержать несколько исключений при обработке коллекции</exception>
        public static void Dispose(this IEnumerable<IDisposable> objs)
        {
            foreach (var obj in objs)
                try
                {
                    obj.Dispose(); 
                } 
                catch { throw; }
        }
        /// <summary>
        /// Получает описание значения перечисления (Enum) из атрибута DescriptionAttribute.
        /// Если атрибут не задан, возвращает строковое представление значения.
        /// </summary>
        /// <param name="value">Значение перечисления</param>
        /// <returns>Описание из атрибута Description или строковое представление значения</returns>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
        /// <summary>
        /// Получает описания всех значений указанного типа перечисления.
        /// </summary>
        /// <typeparam name="T">Тип перечисления</typeparam>
        /// <returns>Коллекция описаний всех значений перечисления</returns>
        public static IEnumerable<string> GetDescriptions<T>() where T : struct, Enum
        {
            return Enum.GetValues<T>().Executes(v => GetDescription(v));
        }

        /// <summary>
        /// Выполняет указанное действие для каждого элемента коллекции.
        /// </summary>
        /// <typeparam name="T">Тип элементов коллекции</typeparam>
        /// <param name="values">Коллекция элементов</param>
        /// <param name="method">Действие для выполнения</param>
        public static void Executes<T>(this IEnumerable<T> values, Action<T> method)
        {
            foreach (T value in values)
                method(value);
        }
        /// <summary>
        /// Выполняет указанное действие заданное количество раз.
        /// </summary>
        /// <param name="method">Действие для выполнения</param>
        /// <param name="repeat">Количество повторений</param>
        public static void Executes(Action method, int repeat)
        {
            for (int i = 0; i < repeat; i++)
            {
                method?.Invoke();
            }
        }
        /// <summary>
        /// Выполняет указанную функцию заданное количество раз и возвращает коллекцию результатов.
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="method">Функция для выполнения</param>
        /// <param name="repeat">Количество повторений</param>
        /// <returns>Коллекция результатов выполнения функции</returns>
        public static IEnumerable<T> Executes<T>(Func<T> method, int repeat)
        {
            ICollection<T> collection = [];
            for (int i = 0; i < repeat; i++)
            {
                collection.Add(method.Invoke());
            }
            return collection;
        }
        /// <summary>
        /// Асинхронно выполняет указанную функцию для каждого элемента коллекции.
        /// </summary>
        /// <typeparam name="T">Тип элементов коллекции</typeparam>
        /// <param name="values">Коллекция элементов</param>
        /// <param name="method">Асинхронная функция для выполнения</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        public static async Task ExecutesAsync<T>(this IEnumerable<T> values, Func<T, Task> method)
        {
            foreach (T value in values)
                await method.Invoke(value);
        }

        /// <summary>
        /// Выполняет указанную функцию для каждого элемента коллекции и возвращает коллекцию результатов.
        /// </summary>
        /// <typeparam name="T">Тип исходных элементов</typeparam>
        /// <typeparam name="TResult">Тип результатов</typeparam>
        /// <param name="values">Коллекция элементов</param>
        /// <param name="method">Функция преобразования</param>
        /// <returns>Коллекция результатов выполнения функции</returns>
        public static IEnumerable<TResult> Executes<T, TResult>(this IEnumerable<T> values, Func<T, TResult> method)
        {
            ICollection<TResult> result = [];
            foreach (T value in values)
                result.Add(method(value));
            return result;
        }
        /// <summary>
        /// Выполняет указанную функцию заданное количество раз и возвращает коллекцию результатов (синтаксис метода расширения для int).
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="repeat">Количество повторений (вызывающий объект)</param>
        /// <param name="method">Функция для выполнения</param>
        /// <returns>Коллекция результатов выполнения функции</returns>
        public static IEnumerable<T> Executes<T>(this int repeat, Func<T> method)
        {
            ICollection<T> collection = [];
            for (int i = 0; i < repeat; i++)
            {
                collection.Add(method.Invoke());
            }
            return collection;
        }
        /// <summary>
        /// Асинхронно выполняет указанную функцию для каждого элемента коллекции и возвращает коллекцию результатов.
        /// </summary>
        /// <typeparam name="T">Тип исходных элементов</typeparam>
        /// <typeparam name="TResult">Тип результатов</typeparam>
        /// <param name="values">Коллекция элементов</param>
        /// <param name="method">Асинхронная функция преобразования</param>
        /// <returns>Задача, результатом которой является коллекция результатов выполнения функции</returns>
        public static async Task<IEnumerable<TResult>> ExecutesAsync<T, TResult>(this IEnumerable<T> values, Func<T, Task<TResult>> method)
        {
            ICollection<TResult> result = [];
            foreach (T value in values)
                result.Add(await method(value));
            return result;
        }
    }
}
