using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace originalstoremada.C_;

public class DBException
{
    public static string GetContraintException(DbUpdateException ex)
    {
        string error = "";
        if (ex.InnerException is PostgresException pgException && pgException.SqlState == "23505")
        {
            error = pgException.Message;
        }

        return error;
    }
}