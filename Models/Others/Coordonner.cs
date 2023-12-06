namespace originalstoremada.Models.Others;

[Serializable]
public class Coordonner
{
    public string Ville { get; set; }
    public string Quartier { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    
    public int IdBoutique { get; set; }
    public int Type { get; set; }
    
    public Boutiques.Boutique Boutique { get; set; }
    
    public double Distance { get; set; }

    public Coordonner()
    {
    }

    public Coordonner(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
    }
}