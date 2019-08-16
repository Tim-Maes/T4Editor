using System;
using System.Collections.Generic;

namespace T4Editor.Extensions
{
    public static class ObjectExtensions
    {
        public static T JsonClone<T>(this T source)
        {
            var serializedObject = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            var deserializedObject = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serializedObject);
            return deserializedObject;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }
    }
}
