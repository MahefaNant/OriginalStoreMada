using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using originalstoremada.Data;
using originalstoremada.Models.Payements;
using originalstoremada.Services.RedirectAttribute;

namespace originalstoremada.Controllers.Payement;

public class TypePayementController : Controller
{
    
    private readonly ApplicationDbContext _context;

    public TypePayementController(ApplicationDbContext context)
    {
        _context = context;
    }

    [CheckAdminLevel5]
    // GET
    public async Task<IActionResult> Index()
    {
        var type = await _context.TypePayement.OrderBy(q => q.Nom).ToListAsync();
        return View(type);
    }

    [HttpPost]
    public IActionResult Create(TypePayement typePayement)
    {
        try
        {
            if (!typePayement.Nom.IsNullOrEmpty() && !typePayement.NumeroResp.IsNullOrEmpty() && !typePayement.NomNum.IsNullOrEmpty() && !typePayement.NomResp.IsNullOrEmpty())
            {
                _context.Add(typePayement);
            }
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            // ignored
        }

        return RedirectToAction(nameof(Index));
    } 
    
    [HttpPost]
    public async Task<IActionResult> Edit(TypePayement typePayement)
    {
        try
        {
            if (typePayement.Id!=0 && !typePayement.Nom.IsNullOrEmpty() && !typePayement.NumeroResp.IsNullOrEmpty() && !typePayement.NomNum.IsNullOrEmpty() && !typePayement.NomResp.IsNullOrEmpty())
            {
                var T = await _context.TypePayement.FindAsync(typePayement.Id);
                if (T != typePayement)
                {
                    T.Nom = typePayement.Nom;
                    T.NumeroResp = typePayement.NumeroResp;
                    T.NomNum = typePayement.NomNum;
                    T.NomResp = typePayement.NomResp;
                    _context.Update(T);
                }
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // ignored
        }

        return RedirectToAction(nameof(Index));
    } 
}