using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    static class ArrayExtension
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
