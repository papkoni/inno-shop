namespace ProductService.Application.Filters;

public class ProductFilter
{
    public string? Title { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? CreatedByUserId { get; set; }
}