namespace originalstoremada.C_;

public class UtilesFonctions
{
    public static double GetPourcent(double val, double reff) {
        return Math.Round((val*100)/reff ,2 );
    }

    public static double GetValue(double val, double pourcent)
    {
        return Math.Round((pourcent*val)/100);
    }

    public static string[] Mois()
    {
        string[] res = new string[12];
        res[0] = "Janvier";
        res[1] = "Février";
        res[2] = "Mars";
        res[3] = "Avril";
        res[4] = "Mais";
        res[5] = "Juin";
        res[6] = "Juillet";
        res[7] = "Août";
        res[8] = "Septembre";
        res[9] = "Octobre";
        res[10] = "Novembre";
        res[11] = "Decembre";
        return res;
    }
}