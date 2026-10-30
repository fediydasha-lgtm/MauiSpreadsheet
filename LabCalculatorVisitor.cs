using System;
using System.Numerics;
using Antlr4.Runtime.Tree;
namespace MauiSpreadsheet;

public class LabCalculatorVisitor : CalculatorBaseVisitor<BigInteger>
{
    private readonly SpreadsheetManager _tableContext;

    public LabCalculatorVisitor(SpreadsheetManager tableContext)
    {
        _tableContext = tableContext;
    }

    public override BigInteger VisitCompileUnit(CalculatorParser.CompileUnitContext context)
    {
        return Visit(context.expr());
    }

    public override BigInteger VisitNumberExpr(CalculatorParser.NumberExprContext context)
    {
        return BigInteger.Parse(context.GetText());
    }

    public override BigInteger VisitIdentifierExpr(CalculatorParser.IdentifierExprContext context)
    {
        string cellName = context.GetText().ToUpper();
        return _tableContext.GetCellValue(cellName);
    }

    public override BigInteger VisitParenthesesExpr(CalculatorParser.ParenthesesExprContext context)
    {
        return Visit(context.expr());
    }

    public override BigInteger VisitUnarExpr(CalculatorParser.UnarExprContext context)
    {
        var number = Visit(context.expr());
        
        if (context.GetChild(0).GetText() == "-")
        {
            return -number;
        }
        return number;
    }

    public override BigInteger VisitIncExpr(CalculatorParser.IncExprContext context)
    {
        return Visit(context.expr()) + 1;
    }

    public override BigInteger VisitDecExpr(CalculatorParser.DecExprContext context)
    {
        return Visit(context.expr()) - 1;
    }

    public override BigInteger VisitMultiplicativeExpr(CalculatorParser.MultiplicativeExprContext context)
    {
        var left = Visit(context.left);
        var right = Visit(context.right);
        var op = context.GetChild(1).GetText().ToLower();

        return op switch
        {
            "*" => left * right,
            "/" => right != 0 ? left / right : throw new DivideByZeroException("Ділення на 0"),
            "div" => right != 0 ? left / right : throw new DivideByZeroException("Ділення на 0 (div)"),
            "mod" => right != 0 ? left % right : throw new DivideByZeroException("Залишок від ділення на 0 (mod)"),
            _ => throw new InvalidOperationException($"Невідомий оператор: {op}")
        };
    }

    public override BigInteger VisitAdditiveExpr(CalculatorParser.AdditiveExprContext context)
    {
        var left = Visit(context.left);
        var right = Visit(context.right);
        var op = context.GetChild(1).GetText();

        return op switch
        {
            "+" => left + right,
            "-" => left - right,
            _ => throw new InvalidOperationException($"Невідомий оператор: {op}")
        };
    }
}