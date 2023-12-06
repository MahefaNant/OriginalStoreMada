namespace originalstoremada.Services;

public class Pagination<T>
{
    
    public int SizeList { get; set; }
    public int PageId { get; set; }
    public int TotalItemsCount { get; set; }
    public int TotalPages { get; set; }

    public readonly IQueryable<T> Query;

    public Pagination()
    {
    }

    public Pagination(int sizeList, int? pagId, IQueryable<T> query)
    {
        SizeList = sizeList;
        PageId = (int)(pagId!=null? pagId : 0);
        PageId = PageId > 0 ? PageId : 1;
        Query = query;
        TotalItemsCount = Query.Count();
        TotalPages = (int)Math.Ceiling((double)TotalItemsCount / SizeList);
    }

    public IQueryable<T> Paginate()
    {
        var res = Query
            .Skip((PageId - 1) * SizeList)
            .Take(SizeList);
        return res;
    }
}