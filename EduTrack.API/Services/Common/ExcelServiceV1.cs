using OfficeOpenXml;

namespace EduTrack.API.Services.Common
{
    public class ExcelColumnMap<T>
    {
        public int ColumnIndex { get; set; }
        public Action<T, object> MapAction { get; set; }
    }

    public class ExcelServiceV1
    {
        public async static Task<List<T>> ReadExcelAsync<T>(IFormFile file, int startRow, int indexSheet, Dictionary<string, ExcelColumnMap<T>> columnMaps) where T : new()
        {
            var result = new List<T>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[indexSheet];

            int row = startRow;

            while (true)
            {
                // check dòng rỗng (cột đầu tiên)
                if (worksheet.Cells[row, 1].Value == null)
                    break;

                var item = new T();

                foreach (var map in columnMaps.Values)
                {
                    var cellValue = worksheet.Cells[row, map.ColumnIndex].Value;
                    map.MapAction(item, cellValue);
                }

                result.Add(item);
                row++;
            }

            return result;
        }
    }
}
