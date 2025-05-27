using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;

namespace LibImageProcessing.Extensions
{
    internal static class DxExtensions
    {
        public static unsafe void DisposeIfNotNull<T>(this ComPtr<T> comPtr)
            where T : unmanaged, IComVtbl<T>
        {
            if (comPtr.Handle != null)
            {
                comPtr.Dispose();
            }
        }
    }
}
