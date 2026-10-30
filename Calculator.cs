using System;
using System.Numerics;
using Antlr4.Runtime;

namespace MauiSpreadsheet;

public class Calculator
{
    public static BigInteger Evaluate(string expression, SpreadsheetManager manager)
    {
        if (string.IsNullOrWhiteSpace(expression)) return 0;

        // Если введен просто текст или число без знака равенства, пытаемся распарсить как выражение
        string input = expression.StartsWith("=") ? expression.Substring(1) : expression;

        var inputStream = new AntlrInputStream(input);
        var lexer = new CalculatorLexer(inputStream);
        lexer.RemoveErrorListeners();

        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new CalculatorParser(commonTokenStream);
        parser.RemoveErrorListeners();

        var context = parser.compileUnit();
        var visitor = new LabCalculatorVisitor(manager);

        return visitor.Visit(context);
    }
}