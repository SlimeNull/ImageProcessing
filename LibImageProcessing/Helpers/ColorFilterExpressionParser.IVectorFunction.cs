namespace LibImageProcessing.Helpers
{
    internal static partial class ColorFilterExpressionParser
    {
        public interface IVectorFunction
        {
            string Name { get; }
            string NameInShader { get; }

            IEnumerable<IVectorFunctionOverride> Overrides { get; }
        }

        public interface IVectorFunctionOverride
        {
            IReadOnlyList<IVectorFunctionArgument> Arguments { get; }
            int ReturnComponents { get; }
        }

        public interface IVectorFunctionArgument
        {
            int InputComponents { get; }
        }
    }
}
