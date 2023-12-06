namespace originalstoremada.Models.Evenements.others;

public class DayInMonth
{
    public int DayNumber { get; set; }
    
    public int ExistEvent { get; set; }
    
    public int ElementCount { get; set; }
    
    public List<Evenement> Evenements { get; set; }
}