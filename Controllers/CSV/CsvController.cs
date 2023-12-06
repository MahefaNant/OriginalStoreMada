using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace originalstoremada.Controllers.CSV;

public class CsvController : Controller
{
    public static async Task<IActionResult> DownloadCsv<T>(List<T> data, string fileName)
    {
        var csvText = "";
        using (var writer = new StringWriter())
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            await csv.WriteRecordsAsync(data);
            csvText = writer.ToString();
        }
        
        csvText = System.Text.RegularExpressions.Regex.Replace(csvText, ",", ";");

        return new FileContentResult(System.Text.Encoding.UTF8.GetBytes(csvText), "text/csv")
        {
            FileDownloadName = $"{fileName}.csv"
        };
    }
}