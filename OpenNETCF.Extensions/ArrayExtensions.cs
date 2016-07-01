using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ArrayExtensions
    {
        public static void Append<T>(this T[] array, T newItem)
        {
            var copy = array.ToList();
            copy.Add(newItem);
            array = copy.ToArray();
        }
    }
}
