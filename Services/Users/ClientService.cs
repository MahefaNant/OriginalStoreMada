using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.Data;
using originalstoremada.Models.Users;
using originalstoremada.Services.Repo;

namespace originalstoremada.Services.Users;

public class ClientService: ServiceRepo<Client>
{

    public override Pagination<Client> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }
    
    public ClientService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public static async Task<Client?> Login(ApplicationDbContext _context, string mail)
    {
        var client = await _context.Client.FirstOrDefaultAsync(u => u.Mail.Contains(mail));
        return client;
    }

    public static bool IsValidPassword(Client? client, string code)
    {
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(code, client?.Code);
        if (client != null) client.Code = null;
        return isPasswordValid;
    }

    public static Client? GetByCookies(string cookies)
    {
        return JsonConvert.DeserializeObject<Client>(cookies);
    }

    public static async Task<bool> IsMailExiste(ApplicationDbContext _context ,string mail)
    {
        bool ver = await _context.Client.AnyAsync(q => q.Mail == mail);
        return ver;
    }
    
    /*public string HashPassword(string password)
    {
        // Generate a salt and hash the password
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return hashedPassword;
    }*/
    
}