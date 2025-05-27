using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingWpf.Helpers
{
    internal class DelegateProgress<T> : IProgress<T>
    {
        private readonly Action<T> _report;

        public DelegateProgress(Action<T> report)
        {
            ArgumentNullException.ThrowIfNull(report);
            _report = report;
        }

        public void Report(T value)
        {
            _report.Invoke(value);
        }
    }
}
