using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingWpf.Extensions
{
    internal static class CollectionExtensions
    {
        public static void Swap<T>(this IList<T> list, int index1, int index2)
        {
            if (index1 < 0 || index1 >= list.Count ||
                index2 < 0 || index2 >= list.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index1 == index2)
            {
                return;
            }

            (list[index1], list[index2]) = (list[index2], list[index1]);
        }
    }
}
