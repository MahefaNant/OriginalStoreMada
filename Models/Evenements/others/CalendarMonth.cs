namespace originalstoremada.Models.Evenements.others;

public class CalendarMonth
{
    public DateTime Date { get; set; }
    public int StartDay { get; set; } // 1 pour lundi, 2 pour mardi, etc.
    public List<DayInMonth> DaysInMonths { get; set; }
}