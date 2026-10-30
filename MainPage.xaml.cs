using System.IO;

namespace MauiSpreadsheet;

public partial class MainPage : ContentPage
{
    private int CountColumn = 5;
    private int CountRow = 10;
    private bool showExpressions = false;
    
    private readonly SpreadsheetManager _manager = new();
    private readonly Dictionary<string, Entry> _entryMap = new();

    public MainPage()
    {
        InitializeComponent();
        CreateGrid();
    }

    private void CreateGrid()
    {
        TableContainer.Children.Clear();
        _entryMap.Clear();

        double cellWidth = 100;
        double cellHeight = 35;
        double headerWidth = 50;

        var headerRow = new HorizontalStackLayout { Spacing = 1 };
        headerRow.Children.Add(CreateCellBorder("", headerWidth, cellHeight, Color.FromArgb("#E5E7EB"), isHeader: true));

        for (int col = 1; col <= CountColumn; col++)
        {
            headerRow.Children.Add(CreateCellBorder(GetColumnName(col), cellWidth, cellHeight, Color.FromArgb("#E5E7EB"), isHeader: true));
        }

        TableContainer.Children.Add(headerRow);

        for (int row = 1; row <= CountRow; row++)
        {
            var dataRow = new HorizontalStackLayout { Spacing = 1 };
            dataRow.Children.Add(CreateCellBorder(row.ToString(), headerWidth, cellHeight, Color.FromArgb("#E5E7EB"), isHeader: true));

            for (int col = 1; col <= CountColumn; col++)
            {
                string cellName = $"{GetColumnName(col)}{row}";
                var cell = _manager.GetCell(cellName);

                var entry = new Entry
                {
                    Text = GetCellDisplayText(cell),
                    Placeholder = "",
                    WidthRequest = cellWidth - 6,
                    HeightRequest = cellHeight - 6,
                    BackgroundColor = cell.HasError ? Color.FromArgb("#FEE2E2") : Colors.White,
                    TextColor = cell.HasError ? Colors.Red : Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 14
                };

                string nameCopy = cellName;
                entry.Unfocused += (s, e) =>
                {
                    _manager.SetCellExpression(nameCopy, entry.Text);
                };

                _entryMap[cellName] = entry;

                var cellBorder = new Border
                {
                    WidthRequest = cellWidth,
                    HeightRequest = cellHeight,
                    BackgroundColor = Colors.White,
                    Stroke = Color.FromArgb("#D1D5DB"),
                    StrokeThickness = 1,
                    Padding = new Thickness(0),
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start,
                    Content = entry
                };

                dataRow.Children.Add(cellBorder);
            }

            TableContainer.Children.Add(dataRow);
        }
    }

    private string GetCellDisplayText(Cell cell)
    {
        if (showExpressions)
        {
            return cell.Expression;
        }

        if (cell.HasError)
        {
            return string.IsNullOrEmpty(cell.ErrorMessage) ? "ERROR" : cell.ErrorMessage;
        }

        if (string.IsNullOrWhiteSpace(cell.Expression))
        {
            return "";
        }

        return cell.Value.ToString();
    }

    private void UpdateUI()
    {
        foreach (var kvp in _entryMap)
        {
            string cellName = kvp.Key;
            Entry entry = kvp.Value;
            var cell = _manager.GetCell(cellName);

            entry.Text = GetCellDisplayText(cell);

            if (!showExpressions && cell.HasError)
            {
                entry.TextColor = Colors.Red;
                entry.BackgroundColor = Color.FromArgb("#FEE2E2");
            }
            else
            {
                entry.TextColor = Colors.Black;
                entry.BackgroundColor = Colors.White;
            }
        }
    }

    private Border CreateCellBorder(string text, double width, double height, Color bgColor, bool isHeader)
    {
        var label = new Label
        {
            Text = text,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            FontAttributes = isHeader ? FontAttributes.Bold : FontAttributes.None,
            TextColor = Colors.Black,
            FontSize = 14
        };

        return new Border
        {
            WidthRequest = width,
            HeightRequest = height,
            BackgroundColor = bgColor,
            Stroke = Color.FromArgb("#9CA3AF"),
            StrokeThickness = 1,
            Padding = new Thickness(0),
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            Content = label
        };
    }

    private string GetColumnName(int index)
    {
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string value = "";
        while (index > 0)
        {
            int remainder = (index - 1) % 26;
            value = letters[remainder] + value;
            index = (index - 1) / 26;
        }
        return value;
    }

    private void btnCalculate_Click(object sender, EventArgs e)
    {
        foreach (var kvp in _entryMap)
        {
            _manager.SetCellExpression(kvp.Key, kvp.Value.Text);
        }

        _manager.EvaluateAll();
        UpdateUI();
    }

    private void btnToggleMode_Click(object sender, EventArgs e)
    {
        showExpressions = !showExpressions;
        UpdateUI();
    }

    private void btnAddRow_Click(object sender, EventArgs e)
    {
        CountRow++;
        CreateGrid();
        UpdateUI();
    }

    private void btnDeleteRow_Click(object sender, EventArgs e)
    {
        if (CountRow > 1)
        {
            CountRow--;
            CreateGrid();
            UpdateUI();
        }
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            foreach (var kvp in _entryMap)
            {
                _manager.SetCellExpression(kvp.Key, kvp.Value.Text);
            }

            string filePath = Path.Combine(FileSystem.AppDataDirectory, "spreadsheet.json");
            _manager.SaveToJson(filePath);
            await DisplayAlert("Успіх", $"Таблицю збережено у:\n{filePath}", "ОК");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти: {ex.Message}", "ОК");
        }
    }

    private async void btnLoad_Click(object sender, EventArgs e)
    {
        try
        {
            string filePath = Path.Combine(FileSystem.AppDataDirectory, "spreadsheet.json");
            if (File.Exists(filePath))
            {
                _manager.LoadFromJson(filePath);
                _manager.EvaluateAll();
                UpdateUI();
                await DisplayAlert("Успіх", "Таблицю успішно завантажено!", "ОК");
            }
            else
            {
                await DisplayAlert("Помилка", "Збережений файл не знайдено.", "ОК");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося завантажити: {ex.Message}", "ОК");
        }
    }

    private async void btnHelp_Click(object sender, EventArgs e)
    {
        string helpText = "Програма 'Електронна таблиця'\n\n" +
                          "Особливості:\n" +
                          "• Підтримка цілих чисел довільної довжини (BigInteger)\n" +
                          "• Операції: +, -, *, /, mod, div\n" +
                          "• Унарний плюс/мінус та функції inc(x), dec(x)\n" +
                          "• Посилання на клітинки (наприклад, A1 + B2)\n\n" +
                          "Кнопки:\n" +
                          "• 'Обчислити' — розраховує значення всіх формул\n" +
                          "• 'Вираз / Значення' — перемикає режим відображення введених формул або результатів";

        await DisplayAlert("Довідка", helpText, "Зрозуміло");
    }
}