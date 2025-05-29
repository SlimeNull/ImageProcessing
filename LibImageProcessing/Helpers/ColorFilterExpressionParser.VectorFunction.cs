namespace LibImageProcessing.Helpers
{
    internal static partial class ColorFilterExpressionParser
    {
        public record VectorFunction(string Name, string NameInShader, IEnumerable<VectorFunctionOverride> Overrides);
        public record VectorFunctionOverride(int ReturnComponents, IReadOnlyList<VectorFunctionArgument> Arguments);
        public record VectorFunctionArgument(int InputComponents);
    }
}
