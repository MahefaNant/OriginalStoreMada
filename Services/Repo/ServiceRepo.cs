using originalstoremada.Data;

namespace originalstoremada.Services.Repo;

public abstract class ServiceRepo<T>
{
    public abstract Pagination<T> Pagination { get; set; }
    public abstract ApplicationDbContext _context { get; set; }
}