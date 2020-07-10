using System;
using System.Collections.Generic;

namespace Toolbox.Trace
{
    /// <summary>
    /// Extensions to the <see cref="IEnumerable<T>"/> interface
    /// </summary>
    static class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                action(enumerator.Current);
            }
        }
    }
}
