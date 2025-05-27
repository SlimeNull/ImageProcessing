namespace LibImageProcessing.Helpers
{
    internal static partial class ColorFilterExpressionParser
    {
        public interface IVectorVariable
        {
            string Name { get; }
            string NameInShader { get; }
            int Components { get; }

            IEnumerable<IVectorVariable> Members { get; }
        }
    }
}
