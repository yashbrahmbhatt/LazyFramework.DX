using System.Data;
using System.IO;
using LazyFramework.Services.Hermes;
using OfficeOpenXml;
namespace LazyFramework.Models
{

    public static class ExcelHandler
    {
        public async static Task<DataSet?> ReadExcelConfigFile(string filePath, LoggerConsumer? logger = null)
        {
            try
            {
                var dataSet = new DataSet(filePath);
                FileInfo fileInfo = new FileInfo(filePath);

                // Ensure the file exists
                if (!fileInfo.Exists)
                {
                    if(logger != null) logger.Log($"File {filePath} does not exist.");
                    return null;
                }

                using (var package = new ExcelPackage(fileInfo))
                {
                    var workbook = package.Workbook;

                    // Iterate through each sheet in the workbook
                    foreach (var worksheet in workbook.Worksheets)
                    {
                        var table = await ProcessExcelSheetAsync(worksheet, logger);
                        if (table != null)
                        {
                            dataSet.Tables.Add(table);
                        }
                    }
                    return dataSet;
                }
            }
            catch (Exception ex)
            {
                if (logger != null) logger.Log($"Error reading Excel config file: {ex.Message}");
                return null;
            }
        }

        private async static Task<DataTable> ProcessExcelSheetAsync(ExcelWorksheet worksheet, LoggerConsumer logger)
        {
            if (logger != null) logger.Log($"Processing sheet: {worksheet.Name}");

            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            // Create a DataTable to store the data
            var dataTable = new DataTable(worksheet.Name);

            // Iterate through each row in the worksheet
            for (int row = 1; row <= rowCount; row++)  // Starting from 1 since EPPlus uses 1-based indexing
            {
                var rowData = new List<string>();

                // Iterate through each column in the row
                for (int col = 1; col <= colCount; col++)  // Starting from 1
                {
                    var cell = worksheet.Cells[row, col];
                    var cellValue = await GetCellValueAsync(cell, logger);

                    // Skip empty cells
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        rowData.Add(cellValue);
                    }
                }

                // Process the data row or header row
                if (row == 1)  // Assuming the first row is the header
                {
                    // Process header row - create columns in DataTable
                    for (int col = 1; col <= colCount; col++)
                    {
                        var headerCell = worksheet.Cells[row, col];
                        var headerValue = await GetCellValueAsync(headerCell, logger);
                        if (!string.IsNullOrWhiteSpace(headerValue))
                        {
                            dataTable.Columns.Add(headerValue);
                        }
                    }
                }
                else
                {
                    // Process data rows - add rows to DataTable
                    var dataRow = dataTable.NewRow();
                    for (int col = 0; col < rowData.Count; col++)
                    {
                        dataRow[col] = rowData[col];
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            // Return the populated DataTable
            return dataTable;
        }


        private async static Task<string> GetCellValueAsync(ExcelRange cell, LoggerConsumer logger)
        {
            return await Task.Run(() =>
            {
                // Check if the cell contains a value or is a shared string
                if (cell.Value != null)
                {
                    // If it's a string, return it directly
                    return cell.Text;
                }

                // Handle any other cell types, such as formulas or dates
                return string.Empty;
            });
        }
    }
}