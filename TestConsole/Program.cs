// 示例使用
using Irony.Parsing;

public class Program
{
    public static void Main()
    {
        TestExpression("rgb.r * 1.1, rgb.g * 0.5, rgb.b * 0.8");
        TestExpression("rgba.r, hsv.v, hsl.l");
        TestExpression("hsv.hv.v");  // 测试嵌套访问
        TestExpression("hsv.hs * 2.0 + hsva.a");
    }

    private static void TestExpression(string expression)
    {
        var grammar = new ColorExpressionParser.ColorFilterExpressionGrammar();
        var parser = new Parser(grammar);

        var parseTree = parser.Parse(expression);

        Console.WriteLine(parseTree.Status);
    }
}
