using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using originalstoremada.Data;
using originalstoremada.Models.Users;
using originalstoremada.Services.Repo;

namespace originalstoremada.Services.Users;

public class AdminService: ServiceRepo<Admin>
{

    public override Pagination<Admin> Pagination { get; set; }
    public override ApplicationDbContext _context { get; set; }
    
    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public static async Task<Admin?> Login(ApplicationDbContext _context, string mail)
    {
        var admin = await _context.Admin.FirstOrDefaultAsync(u => u.Mail.Contains(mail));
        return admin;
    }

    public static bool IsValidPassword(Admin? admin, string code)
    {
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(code, admin?.Code);
        if (admin != null) admin.Code = null;
        return isPasswordValid;
    }

    public static bool IsLevel_5(Admin? admin )
    {
        return admin.Niveau == 5;
    }

    public static Admin? GetByCookies(string cookies)
    {
        return JsonConvert.DeserializeObject<Admin>(cookies);
    }

    public static async Task<bool> IsMailExiste(ApplicationDbContext _context ,string mail)
    {
        bool ver = await _context.Admin.AnyAsync(q => q.Mail == mail);
        return ver;
    }

    public static async Task<List<Admin>> AdminWithAffectation(ApplicationDbContext context ,List<Admin> admins)
    {
        foreach (var A in admins)
        {
            var affectEmpl = await context.AffectationEmployer
                .Include( q => q.Boutique)
                .Where(q => q.IdAdmin == A.Id && q.DateFin==null)
                .OrderByDescending(q => q.DateDeb )
                .FirstOrDefaultAsync();
            if (affectEmpl != null) 
                A.AffectationEmployer = affectEmpl;
        }

        return admins;
    } 

}