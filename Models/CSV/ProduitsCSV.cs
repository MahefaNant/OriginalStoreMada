using CsvHelper.Configuration.Attributes;

namespace originalstoremada.Models.CSV;

public class ProduitsCSV
{
    [Name("categorie")]
    public string Categorie { get; set; }
    
    [Name("genre")]
    public string Genre { get; set; }
    
    [Name("designation")]
    public string Nom { get; set; }
    
    [Name("fournisseur")]
    public string Fournisseur { get; set; }
    
    [Name("achat")]
    public string PrixAchat { get; set; }
    
    [Name("vente")]
    public string PrixVente { get; set; }
    
    [Name("description")]
    public string Description { get; set; }

    [Name("pourEnfant")]
    public string PourEnfant = "N";
}