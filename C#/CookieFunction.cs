namespace originalstoremada.C_;

public class CookieFunction
{
    public static CookieOptions OptionDay(int days)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddDays(days)
        };
        return cookieOptions;
    }
    
    public static CookieOptions OptionYear(int years)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddYears(years)
        };
        return cookieOptions;
    }
    
    public static CookieOptions OptionHour(int h)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddHours(h)
        };
        return cookieOptions;
    }
    
    public static CookieOptions OptionMinute(int min)
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddMinutes(min)
        };
        return cookieOptions;
    }
    
}