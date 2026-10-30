using System.Numerics;

namespace MauiSpreadsheet;

public class Cell
{
    public string Name { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
    public BigInteger Value { get; set; } = 0;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool HasError { get; set; } = false;

    public Cell(string name)
    {
        Name = name;
    }

    public void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
        Value = 0;
    }
}