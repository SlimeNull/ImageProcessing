using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Irony.Parsing;
using Silk.NET.Core.Native;
using static LibImageProcessing.Helpers.ColorFilterExpressionParser;

namespace LibImageProcessing.Helpers
{
    internal static partial class ColorFilterExpressionParser
    {
        private static readonly ColorFilterExpressionGrammar s_grammar;
        private static readonly Parser s_parser;

        static ColorFilterExpressionParser()
        {
            s_grammar = new();
            s_parser = new Parser(s_grammar);
        }

        public record struct ValueNodeInfo(string Text, int Components);

        private static IEnumerable<string> RepeatString(string value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return value;
            }
        }
        private static IEnumerable<ValueNodeInfo> ExpressionListNodeInfo(ParseTreeNode node, VectorVariable[] availableVariables, VectorFunction[] availableFunctions)
        {
            ParseTreeNode? nextNode = node;

            while (nextNode is not null)
            {
                switch (nextNode.ChildNodes)
                {
                    case [var expression]:
                        yield return ExpressionNodeInfo(expression, availableVariables, availableFunctions);
                        yield break;
                    case [var expressionList, var expression]:
                        foreach (var before in ExpressionListNodeInfo(expressionList, availableVariables, availableFunctions))
                        {
                            yield return before;
                        }

                        yield return ExpressionNodeInfo(expression, availableVariables, availableFunctions);
                        yield break;
                }
            }
        }
        private static IEnumerable<ValueNodeInfo> ArgumentListNodeInfo(ParseTreeNode node, VectorVariable[] availableVariables, VectorFunction[] availableFunctions)
        {
            ParseTreeNode? nextNode = node;

            while (nextNode is not null)
            {
                switch (nextNode.ChildNodes)
                {
                    case [var expression]:
                        yield return ExpressionNodeInfo(expression, availableVariables, availableFunctions);
                        yield break;
                    case [var expressionList, var expression]:
                        foreach (var before in ArgumentListNodeInfo(expressionList, availableVariables, availableFunctions))
                        {
                            yield return before;
                        }

                        yield return ExpressionNodeInfo(expression, availableVariables, availableFunctions);
                        yield break;
                }
            }
        }

        private static ValueNodeInfo ExpressionNodeInfo(ParseTreeNode node, VectorVariable[] availableVariables, VectorFunction[] availableFunctions)
        {
            switch (node.Term.Name)
            {
                case "number":
                    return new ValueNodeInfo(node.Token.Text, 1);

                case "identifier":
                    return IdentifierNodeInfo(node, availableVariables);

                case "memberAccess":
                    return MemberAccessNodeInfo(node, availableVariables);

                case "functionCall":
                    return FunctionCallNodeInfo(node, availableVariables, availableFunctions);

                case "factor":
                    switch (node.ChildNodes.Count)
                    {
                        case 1:
                            return ExpressionNodeInfo(node.ChildNodes[0], availableVariables, availableFunctions);
                        case 3:
                            var childInfo = ExpressionNodeInfo(node.ChildNodes[1], availableVariables, availableFunctions);
                            return new ValueNodeInfo($"({childInfo.Text})", childInfo.Components);
                        default:
                            throw new InvalidOperationException();
                    }

                case "term":
                    switch (node.ChildNodes.Count)
                    {
                        case 1:
                            return ExpressionNodeInfo(node.ChildNodes[0], availableVariables, availableFunctions);
                        case 3:
                            var childInfo1 = ExpressionNodeInfo(node.ChildNodes[0], availableVariables, availableFunctions);
                            var childInfo2 = ExpressionNodeInfo(node.ChildNodes[2], availableVariables, availableFunctions);
                            if (childInfo1.Components != 1 &&
                                childInfo2.Components != 1)
                            {
                                throw new ArgumentException("Invalid term");
                            }

                            var op = node.ChildNodes[1].Token.Text;
                            return new ValueNodeInfo($"{childInfo1.Text} {op} {childInfo2.Text}", childInfo1.Components * childInfo2.Components);
                        default:
                            throw new InvalidOperationException();
                    }

                case "expression":
                    switch (node.ChildNodes.Count)
                    {
                        case 1:
                            return ExpressionNodeInfo(node.ChildNodes[0], availableVariables, availableFunctions);
                        case 3:
                            var childInfo1 = ExpressionNodeInfo(node.ChildNodes[0], availableVariables, availableFunctions);
                            var childInfo2 = ExpressionNodeInfo(node.ChildNodes[2], availableVariables, availableFunctions);
                            if (childInfo1.Components != childInfo2.Components)
                            {
                                throw new ArgumentException("Invalid term");
                            }

                            var op = node.ChildNodes[1].Token.Text;
                            return new ValueNodeInfo($"{childInfo1.Text} {op} {childInfo2.Text}", childInfo1.Components);
                        default:
                            throw new InvalidOperationException();
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        private static ValueNodeInfo IdentifierNodeInfo(ParseTreeNode node, VectorVariable[] availableVariables)
        {
            var identifier = node.Token.Text;
            foreach (var variable in availableVariables)
            {
                if (variable.Name == identifier)
                {
                    return new ValueNodeInfo(variable.NameInShader, variable.Components);
                }
            }

            throw new ArgumentException($"No variable like '{identifier}'");
        }

        private static ValueNodeInfo MemberAccessNodeInfo(ParseTreeNode node, VectorVariable[] availableVariables)
        {
            List<string> accessSequence = new List<string>();

            ParseTreeNode? nextNode = node;
            while (nextNode is not null)
            {
                switch (nextNode.ChildNodes)
                {
                    case [var identifier]:
                        accessSequence.Add(identifier.Token.Text);
                        nextNode = null;
                        break;
                    case [var memberAccess, var identifier]:
                        accessSequence.Add(identifier.Token.Text);
                        nextNode = memberAccess;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            var components = 0;
            IEnumerable<VectorVariable> currentVariables = availableVariables;
            List<string> shaderAccessSequence = new List<string>();
            for (int i = accessSequence.Count - 1; i >= 0; i--)
            {
                bool matched = false;
                foreach (var variable in currentVariables)
                {
                    if (variable.Name == accessSequence[i])
                    {
                        shaderAccessSequence.Add(variable.NameInShader);
                        currentVariables = variable.Members;
                        components = variable.Components;
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    throw new ArgumentException($"No variable like '{accessSequence[i]}'");
                }
            }

            var shaderCode = string.Join('.', shaderAccessSequence);
            return new ValueNodeInfo(shaderCode, components);
        }

        private static ValueNodeInfo FunctionCallNodeInfo(ParseTreeNode node, VectorVariable[] availableVariables, VectorFunction[] availableFunctions)
        {
            switch (node.ChildNodes)
            {
                case [var identifier, var argumentList]:
                    var identifierText = identifier.Token.Text;
                    var argumentListNodeInfos = ArgumentListNodeInfo(argumentList, availableVariables, availableFunctions).ToArray();
                    if (availableFunctions.FirstOrDefault(f => f.Name == identifierText) is not { } matchedFunc)
                    {
                        throw new ArgumentException($"No function like '{identifierText}'");
                    }

                    if (matchedFunc.Overrides.FirstOrDefault(ovrd => ovrd.Arguments.Count == argumentListNodeInfos.Length) is not { } matchedOverride)
                    {
                        throw new ArgumentException($"Function '{identifierText}' no override receives {argumentListNodeInfos.Length} arguments");
                    }

                    for (int i = 0; i < matchedOverride.Arguments.Count; i++)
                    {
                        var currentArgument = matchedOverride.Arguments[i];
                        if (currentArgument.InputComponents != argumentListNodeInfos[i].Components)
                        {
                            throw new ArgumentException($"Function '{identifierText}' with {argumentListNodeInfos.Length} arguments override, argument index {i}, component count not match, required: {currentArgument.InputComponents}, actual: {argumentListNodeInfos[i].Components}");
                        }
                    }

                    return new ValueNodeInfo($"{identifierText}({string.Join(", ", argumentListNodeInfos.Select(nodeInfo => nodeInfo.Text))})", matchedOverride.ReturnComponents);

                default:
                    throw new InvalidOperationException();
            }
        }


        public delegate string ShaderExpressionDefaultComponentResolver(ValueNodeInfo[] Values, int RequiredComponentIndex);

        public static string GetShaderExpression(string expression, VectorVariable[] availableVariables, VectorFunction[] availableFunctions, ShaderExpressionDefaultComponentResolver defaultComponentResolver)
        {
            var parseTree = s_parser.Parse(expression);
            if (parseTree.HasErrors())
            {
                throw new ArgumentException("Invalid expression");
            }

            var nodeInfos = ExpressionListNodeInfo(parseTree.Root, availableVariables, availableFunctions).ToArray();

            if (nodeInfos.Length == 0)
            {
                throw new ArgumentException("Invalid expression");
            }
            else if (nodeInfos.Length == 1)
            {
                var onePart = nodeInfos[0];
                return onePart.Components switch
                {
                    1 => $"float4({onePart.Text}, {defaultComponentResolver.Invoke(nodeInfos, 1)}, {defaultComponentResolver.Invoke(nodeInfos, 2)}, {defaultComponentResolver.Invoke(nodeInfos, 3)})",
                    2 => $"float4({onePart.Text}, {defaultComponentResolver.Invoke(nodeInfos, 2)}, {defaultComponentResolver.Invoke(nodeInfos, 3)})",
                    3 => $"float4({onePart.Text}, {defaultComponentResolver.Invoke(nodeInfos, 3)})",
                    4 => $"{onePart.Text}",
                    _ => throw new ArgumentException("Invalid expression")
                };
            }
            else
            {
                var totalComponents = 0;
                for (int i = 0; i < nodeInfos.Length; i++)
                {
                    var part = nodeInfos[i];
                    if (part.Components + totalComponents > 4)
                    {
                        throw new ArgumentException("Invalid expression");
                    }

                    totalComponents += part.Components;
                }

                var componentsToFill = Enumerable.Range(totalComponents, 4 - totalComponents)
                    .Select(requiredComponentIndex => defaultComponentResolver.Invoke(nodeInfos, requiredComponentIndex));

                return $"float4({string.Join(", ", nodeInfos.Select(part => part.Text))}, {string.Join(", ", componentsToFill)})";
            }
        }

        public static string GetShaderExpressionForFilter(string expression, ShaderExpressionDefaultComponentResolver defaultComponentResolver)
        {
            VectorVariable[] vectorVariables =
            [
                new VectorVariable("rgb", "rgb", 3, ['r', 'g', 'b']),
                new VectorVariable("rgba", "rgba", 4, ['r', 'g', 'b', 'a']),
                new VectorVariable("hsv", "hsv", 3, ['h', 's', 'v']),
                new VectorVariable("hsva", "hsva", 4, ['h', 's', 'v', 'a']),
                new VectorVariable("hsl", "hsl", 3, ['h', 's', 'l']),
                new VectorVariable("hsla", "hsla", 4, ['h', 's', 'l', 'a']),
            ];

            VectorFunction[] vectorFunctions =
            [

            ];

            return GetShaderExpression(expression, vectorVariables, vectorFunctions, defaultComponentResolver);
        }

        public static string GetShaderExpressionForRgbFilter(string expression)
        {
            return GetShaderExpressionForFilter(expression, (componentsExists, requiredComponentIndex) =>
            {
                if (componentsExists.Length == 1 &&
                    componentsExists[0].Components == 1)
                {
                    return componentsExists[0].Text;
                }

                return "1";
            });
        }

        public static string GetShaderExpressionForHsvFilter(string expression)
        {
            return GetShaderExpressionForFilter(expression, (componentsExists, requiredComponentIndex) =>
            {
                return requiredComponentIndex switch
                {
                    1 => "1",
                    2 => "1",
                    3 => "1",
                    _ => "0"
                };
            });
        }

        public static string GetShaderExpressionForHslFilter(string expression)
        {
            return GetShaderExpressionForFilter(expression, (componentsExists, requiredComponentIndex) =>
            {
                return requiredComponentIndex switch
                {
                    1 => "1",
                    2 => "0.5",
                    3 => "1",
                    _ => "0"
                };
            });
        }
    }
}
