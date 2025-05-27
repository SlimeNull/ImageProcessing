using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;

namespace LibImageProcessing.Helpers
{
    internal static class ShaderColorExpressionParser
    {
        public interface IVectorVariable
        {
            string Name { get; }
            string NameInShader { get; }
            int Components { get; }

            IEnumerable<IVectorVariable> Members { get; }
        }

        public class VectorVariable : IVectorVariable
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

            public IEnumerable<IVectorVariable> Members
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

            private static IEnumerable<IVectorVariable> GenerateCombinationsRecursive(char[] chars, string current, string currentInShader, int length)
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

        private record struct ExpressionPart(string ShaderCode, int Components);

        private static IEnumerable<string> RepeatString(string value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return value;
            }
        }

        private static bool GetExpressionPart(List<string> memberAccessSequence, IVectorVariable[] availableVariables, out ExpressionPart expressionPart)
        {
            List<string> shaderMemberAccessSequence = new List<string>();
            int finalComponents = 0;

            IEnumerable<IVectorVariable> variables = availableVariables;
            foreach (var item in memberAccessSequence)
            {
                if (variables is null)
                {
                    expressionPart = default;
                    return false;
                }

                bool matched = false;
                foreach (var variable in variables)
                {
                    if (variable.Name == item)
                    {
                        shaderMemberAccessSequence.Add(variable.NameInShader);
                        finalComponents = variable.Components;
                        matched = true;
                        variables = variable.Members;
                        break;
                    }
                }

                if (!matched)
                {
                    expressionPart = default;
                    return false;
                }
            }

            expressionPart = new ExpressionPart(string.Join(".", shaderMemberAccessSequence), finalComponents);
            return shaderMemberAccessSequence.Count > 0;
        }

        public static bool GetShaderExpression(string expression, IVectorVariable[] availableVariables, [NotNullWhen(true)] out string? shaderExpression)
        {
            List<ExpressionPart> expressionParts = new();
            List<string> memberAccessSequence = new List<string>();
            StringBuilder identifierBuffer = new StringBuilder();

            foreach (var c in expression)
            {
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (char.IsAsciiLetter(c))
                {
                    identifierBuffer.Append(c);
                }
                else if (c == '.')
                {
                    memberAccessSequence.Add(identifierBuffer.ToString());
                    identifierBuffer.Clear();
                }
                else if (c == ',')
                {
                    memberAccessSequence.Add(identifierBuffer.ToString());
                    identifierBuffer.Clear();
                    if (!GetExpressionPart(memberAccessSequence, availableVariables, out var expressionPart))
                    {
                        shaderExpression = null;
                        return false;
                    }

                    expressionParts.Add(expressionPart);
                }
            }

            if (identifierBuffer.Length > 0)
            {
                memberAccessSequence.Add(identifierBuffer.ToString());
            }

            if (memberAccessSequence.Count != 0)
            {
                if (!GetExpressionPart(memberAccessSequence, availableVariables, out var finalExpressionPart))
                {
                    shaderExpression = null;
                    return false;
                }

                expressionParts.Add(finalExpressionPart);
            }

            if (expressionParts.Count == 0)
            {
                shaderExpression = null;
                return false;
            }
            else if (expressionParts.Count == 1)
            {
                var onePart = expressionParts[0];
                shaderExpression = onePart.Components switch
                {
                    1 => $"float4({onePart.ShaderCode}, {onePart.ShaderCode}, {onePart.ShaderCode}, 1)",
                    2 => $"float4({onePart.ShaderCode}, 1, 1)",
                    3 => $"float4({onePart.ShaderCode}, 1)",
                    4 => $"{onePart.ShaderCode}",
                    _ => null
                };

                return shaderExpression is not null;
            }
            else
            {
                var totalComponents = 0;
                for (int i = 0; i < expressionParts.Count; i++)
                {
                    var part = expressionParts[i];
                    if (part.Components + totalComponents > 4)
                    {
                        shaderExpression = null;
                        return false;
                    }

                    totalComponents += part.Components;
                }

                shaderExpression = $"float4({string.Join(", ", expressionParts.Select(part => part.ShaderCode))}, {string.Join(", ", RepeatString("1", 4 - totalComponents))}";
                return true;
            }
        }

        public static bool GetShaderExpressionForFilter(string expression, [NotNullWhen(true)] out string? shaderExpression)
        {
            IVectorVariable[] vectorVariables =
            [
                new VectorVariable("rgb", "rgb", 3, ['r', 'g', 'b']),
                new VectorVariable("rgba", "rgba", 4, ['r', 'g', 'b', 'a']),
                new VectorVariable("hsv", "hsv", 3, ['h', 's', 'v']),
                new VectorVariable("hsva", "hsva", 4, ['h', 's', 'v', 'a']),
                new VectorVariable("hsl", "hsl", 3, ['h', 's', 'l']),
                new VectorVariable("hsla", "hsla", 4, ['h', 's', 'l', 'a']),
            ];

            return GetShaderExpression(expression, vectorVariables, out shaderExpression);
        }
    }
}
