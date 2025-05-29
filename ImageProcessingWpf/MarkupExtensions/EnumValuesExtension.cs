using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace ImageProcessingWpf.MarkupExtensions
{
    public class EnumValuesExtension : MarkupExtension
    {
        public Type? Type { get; set; }

        public EnumValuesExtension() { }
        public EnumValuesExtension(Type type)
        {
            Type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type is null)
            {
                return Binding.DoNothing;
            }

            return Enum.GetValues(Type);
        }
    }
}
