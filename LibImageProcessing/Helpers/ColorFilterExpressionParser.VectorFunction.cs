namespace LibImageProcessing.Helpers
{
    internal static partial class ColorFilterExpressionParser
    {
        public record VectorFunction(string Name, string NameInShader, IReadOnlyList<VectorFunctionOverride> Overrides);
        public record VectorFunctionOverride(int ReturnComponents, IReadOnlyList<int> ArgumentComponents);
    }
}
