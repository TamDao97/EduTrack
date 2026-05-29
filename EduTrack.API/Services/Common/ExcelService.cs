using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;

public partial class ExcelService
{
    public ExcelService()
    {
    }

    // ============================================================
    // 1. EXPORT TEMPLATE
    // ============================================================
    public byte[] GenerateTemplateVer01(Dictionary<string, string> headers, string sheetName = "Template")
    {
        using (var package = new ExcelPackage())
        {
            var ws = package.Workbook.Worksheets.Add(sheetName);

            int col = 1;
            foreach (var header in headers)
            {
                ws.Cells[1, col].Value = header.Value;

                ws.Cells[1, col].Style.Font.Bold = true;
                ws.Cells[1, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);
                ws.Column(col).Width = 25;
                col++;
            }

            return package.GetAsByteArray();
        }
    }

    // ============================================================
    // 2. IMPORT MAPPING + VALIDATE
    // ============================================================
    public ImportResult<T> ImportWithMapping<T>(Stream stream, Dictionary<string, string> headerMapping, List<ImportRule> rules) where T : new()
    {
        var result = new ImportResult<T>();
        var list = new List<T>();

        using (var package = new ExcelPackage(stream))
        {
            var ws = package.Workbook.Worksheets[0];
            int rowCount = ws.Dimension.End.Row;
            int colCount = ws.Dimension.End.Column;

            var excelHeaders = new Dictionary<int, string>();
            for (int col = 1; col <= colCount; col++)
                excelHeaders[col] = ws.Cells[1, col].Text.Trim();

            for (int row = 2; row <= rowCount; row++)
            {
                var obj = new T();
                var rowErrors = new List<string>();

                foreach (var map in headerMapping)
                {
                    string excelHeader = map.Key;
                    string propertyName = map.Value;

                    var colIndex = excelHeaders.FirstOrDefault(x => x.Value == excelHeader).Key;
                    if (colIndex == 0) continue;

                    string cellValue = ws.Cells[row, colIndex].Text?.Trim();
                    var prop = typeof(T).GetProperty(propertyName);

                    // validate
                    foreach (var rule in rules.Where(r => r.Property == propertyName))
                    {
                        string validationError = rule.Validate(cellValue);
                        if (validationError != null)
                            rowErrors.Add($"{excelHeader} (Dòng {row}): {validationError}");
                    }

                    // set value
                    if (prop != null && !string.IsNullOrEmpty(cellValue))
                    {
                        try
                        {
                            object convertedValue = ConvertToType(cellValue, prop.PropertyType);
                            prop.SetValue(obj, convertedValue);
                        }
                        catch
                        {
                            rowErrors.Add($"{excelHeader} (Dòng {row}): dữ liệu không đúng kiểu");
                        }
                    }
                }

                if (rowErrors.Any())
                    result.Errors.AddRange(rowErrors);
                else
                    list.Add(obj);
            }
        }

        result.Data = list;
        return result;
    }

    private object ConvertToType(string value, Type type)
    {
        if (type == typeof(int))
            return int.Parse(value);

        if (type == typeof(double))
            return double.Parse(value);

        if (type == typeof(DateTime))
            return DateTime.Parse(value);

        return Convert.ChangeType(value, type);
    }

    // ============================================================
    // 3. SUPPORT CLASS
    // ============================================================
    public class ImportResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public List<string> Errors { get; set; } = new List<string>();
        public bool HasError => Errors.Any();
    }

    public class ImportRule
    {
        public string Property { get; set; }
        public bool Required { get; set; } = false;
        public int? MaxLength { get; set; }
        public int? MinLength { get; set; }
        public List<string>? InList { get; set; }

        public string Validate(string value)
        {
            if (Required && string.IsNullOrWhiteSpace(value))
                return "Không được để trống";

            if (MinLength.HasValue && value?.Length < MinLength)
                return $"Tối thiểu {MinLength} ký tự";

            if (MaxLength.HasValue && value?.Length > MaxLength)
                return $"Không vượt quá {MaxLength} ký tự";

            if (InList != null && value != null && !InList.Contains(value))
                return $"Chỉ được: {string.Join(", ", InList)}";

            return null;
        }
    }
}
