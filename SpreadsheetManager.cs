using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace MauiSpreadsheet;

public class SpreadsheetManager
{
    private readonly Dictionary<string, Cell> cells = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, Cell> Cells => cells;

    public void SetCellExpression(string name, string expr)
    {
        string key = name.ToUpper();
        if (!cells.ContainsKey(key))
        {
            cells[key] = new Cell(key);
        }
        cells[key].Expression = expr ?? string.Empty;
        cells[key].ClearError();
    }

    public BigInteger GetCellValue(string name)
    {
        string key = name.ToUpper();
        if (cells.TryGetValue(key, out var cell))
        {
            if (cell.HasError)
            {
                return 0;
            }
            return cell.Value;
        }
        return 0;
    }

    public Cell GetCell(string name)
    {
        string key = name.ToUpper();
        if (!cells.TryGetValue(key, out var cell))
        {
            cell = new Cell(key);
            cells[key] = cell;
        }
        return cell;
    }

    public void EvaluateAll()
    {
        foreach (var cell in cells.Values)
        {
            if (string.IsNullOrWhiteSpace(cell.Expression))
            {
                cell.Value = 0;
                cell.ClearError();
                continue;
            }

            try
            {
                cell.Value = Calculator.Evaluate(cell.Expression, this);
                cell.ClearError();
            }
            catch (Exception ex)
            {
                cell.SetError(ex.Message);
            }
        }
    }

    public void SaveToJson(string filepath)
    {
        var data = new Dictionary<string, string>();
        foreach (var kvp in cells)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value.Expression))
            {
                data[kvp.Key] = kvp.Value.Expression;
            }
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(data, options);
        File.WriteAllText(filepath, json);
    }

    public void LoadFromJson(string filepath)
    {
        if (!File.Exists(filepath)) return;

        string json = File.ReadAllText(filepath);
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        cells.Clear();
        if (data != null)
        {
            foreach (var kvp in data)
            {
                SetCellExpression(kvp.Key, kvp.Value);
            }
        }
    }
}