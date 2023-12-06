namespace originalstoremada.Models.Others;

[Serializable]
public class InfoPayementShop
{
    public double PrixParKm { get; set; }
    public double PrixMinKm { get; set; }
    
    public int Type { get; set; }
    public double Distance { get; set; }
    
    public double MontantPrix { get; set; }
    
    public double FraisLivraison()
    {
        double frais = 0;
        if (Type == 1)
        {
            frais = Distance * PrixParKm; 
            if (frais < PrixMinKm) frais = PrixMinKm;
        }

        return Math.Round(frais, 2);
    }

    public double MontantFinal()
    {
        return Math.Round( MontantPrix + FraisLivraison() ,2);
    }
}