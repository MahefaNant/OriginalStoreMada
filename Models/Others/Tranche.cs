namespace originalstoremada.Models.Others;

public class Tranche
{
    public double Valeur {get; set; }
    public double Pourcent { get; set; }
    public bool EstPayeeSimulate = false;
    public bool EstPayee = false;

    public Tranche(double valeur, double pourcent)
    {
        this.Valeur = valeur;
        this.Pourcent = pourcent;
    }
}