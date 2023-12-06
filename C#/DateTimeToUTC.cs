using TimeZoneConverter;

namespace originalstoremada.C_;

public class DateTimeToUTC
{
    public static DateTime Make(DateTime dateTime)
    {
        dateTime =DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
        dateTime = dateTime.ToUniversalTime();
        return dateTime;
    }

    public static DateTime UpdateTime(DateTime dateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(dateTime);
    }

    public static DateTime DTNow()
    {
        var timeZoneId = "Indian/Antananarivo"; // Nom du fuseau horaire pour Antananarivo, Madagascar
        var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneId);
        return TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
    }
}