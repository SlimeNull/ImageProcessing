namespace LibImageProcessing.Helpers
{
    internal static partial class ColorFilterExpressionParser
    {
        public class VectorVariable
        {
            private readonly char[] _componentNames;

            public VectorVariable(string name, string nameInShader, int components, char[] componentNames)
            {
                ArgumentNullException.ThrowIfNull(name);
                ArgumentNullException.ThrowIfNull(nameInShader);
                ArgumentNullException.ThrowIfNull(componentNames);

                if (components > 4)
                {
                    throw new ArgumentOutOfRangeException(nameof(components));
                }

                if (componentNames.Length != components)
                {
                    throw new ArgumentException(nameof(componentNames));
                }

                Name = name;
                NameInShader = nameInShader;
                Components = components;

                _componentNames = componentNames;
            }

            public string Name { get; }
            public string NameInShader { get; }
            public int Components { get; }

            public IEnumerable<VectorVariable> Members
            {
                get
                {
                    for (int len = 1; len <= Components; len++)
                    {
                        foreach (var item in GenerateCombinationsRecursive(_componentNames, string.Empty, string.Empty, len))
                        {
                            yield return item;
                        }
                    }
                }
            }

            private static IEnumerable<VectorVariable> GenerateCombinationsRecursive(char[] chars, string current, string currentInShader, int length)
            {
                if (current.Length == length)
                {
                    // 如果当前长度达到目标长度，将结果存储起来
                    yield return new VectorVariable(current, currentInShader, currentInShader.Length, current.ToCharArray());
                    yield break;
                }

                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];
                    char cInShader = "xyzw"[i];
                    // 对每一个字符继续递归，生成更多的组合

                    foreach (var item in GenerateCombinationsRecursive(chars, current + c, currentInShader + cInShader, length))
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
