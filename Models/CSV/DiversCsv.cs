using CsvHelper.Configuration.Attributes;

namespace originalstoremada.Models.CSV;

public class DiversCsv
{
    [Name("type")]
    public string Type { get; set; }
    
    [Name("description")]
    public string Corps { get; set; }
    
    [Name("montant")]
    public string Montant { get; set; }
    
    [Name("date")]
    [Format("dd/MM/yyyy HH:mm")]
    public DateTime Date { get; set; }
}