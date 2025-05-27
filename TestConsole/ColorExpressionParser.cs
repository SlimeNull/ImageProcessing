using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

public class ColorExpressionParser
{
    public class ColorFilterExpressionGrammar : Grammar
    {
        public ColorFilterExpressionGrammar() : base(caseSensitive: true)
        {
            // 终结符
            var number = new NumberLiteral("number");
            var identifier = new IdentifierTerminal("identifier");

            // 非终结符
            var memberAccess = new NonTerminal("memberAccess");
            var factor = new NonTerminal("factor");
            var term = new NonTerminal("term");
            var expression = new NonTerminal("expression");
            var expressionList = new NonTerminal("expressionList");

            // 规则定义
            // 成员访问: identifier(.identifier)*
            memberAccess.Rule = identifier | memberAccess + "." + identifier;

            // 因子: 数字、标识符(含成员访问)、括号内的表达式
            factor.Rule = number | memberAccess | "(" + expression + ")";

            // 项: 因子 (* 或 / 因子)*
            term.Rule = factor | term + "*" + factor | term + "/" + factor;

            // 表达式: 项 (+ 或 - 项)*
            expression.Rule = term | expression + "+" + term | expression + "-" + term;

            // 表达式列表: 表达式(, 表达式)*
            expressionList.Rule = expression | expressionList + "," + expression;

            // 设置根节点
            this.Root = expressionList;

            // 设置运算符优先级和结合性
            RegisterOperators(1, "+", "-");
            RegisterOperators(2, "*", "/");

            // 标记标点符号
            MarkPunctuation("(", ")", ".", ",");

            // 设置注释等
            LanguageFlags = LanguageFlags.CreateAst;
        }
    }
}
