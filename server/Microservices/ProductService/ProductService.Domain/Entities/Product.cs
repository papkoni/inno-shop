namespace ProductService.Domain.Entities;

public class Product
{
    public Product(
        string title,
        string description,
        decimal price,
        Guid createdByUserId,
        bool isAvailable = true)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Price = price;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        IsAvailable = isAvailable;
    }
    
    protected Product() { }
    
    public Guid Id { get; private set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; private set; }
}