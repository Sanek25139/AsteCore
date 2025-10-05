using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AsteCore
{
    public static class Tool
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
        public static IEnumerable<string> GetDescriptions<T>() where T : struct, Enum
        {
            return Enum.GetValues<T>().Executes(v => GetDescription(v));
        }


        public static void Executes<T>(this IEnumerable<T> values, Action<T> method)
        {
            foreach (T value in values)
                method(value);
        }

        public static void Executes(Action method, int repeat)
        {
            for (int i = 0; i < repeat; i++)
            {
                method?.Invoke();
            }
        }

        public static ICollection<T> Executes<T>(Func<T> method, int repeat)
        {
            ICollection<T> collection = [];
            for (int i = 0; i < repeat; i++)
            {
                collection.Add(method.Invoke());
            }
            return collection;
        }

        public static void Executes(this int repeat, Action method)
        {
            for (int i = 0; i < repeat; i++)
            {
                method?.Invoke();
            }
        }

        public static async Task ExecutesAsync<T>(this IEnumerable<T> values, Func<T, Task> method)
        {
            foreach (T value in values)
                await method.Invoke(value);
        }

        public static ICollection<TResult> Executes<T, TResult>(this IEnumerable<T> values, Func<T, TResult> method)
        {
            ICollection<TResult> result = [];
            foreach (T value in values)
                result.Add(method(value));
            return result;
        }

        public static ICollection<T> Executes<T>(this int repeat, Func<T> method)
        {
            ICollection<T> collection = [];
            for (int i = 0; i < repeat; i++)
            {
                collection.Add(method.Invoke());
            }
            return collection;
        }


        public static async Task<ICollection<TResult>> ExecutesAsync<T, TResult>(this IEnumerable<T> values, Func<T, Task<TResult>> method)
        {
            ICollection<TResult> result = [];
            foreach (T value in values)
                result.Add(await method(value));
            return result;
        }
    }
}
