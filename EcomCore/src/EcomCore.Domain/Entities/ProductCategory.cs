namespace EcomCore.Domain.Entities;

public class ProductCategory : BaseEntity, IEntity<int>
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public Product Product { get; set; } = null!;

    public Category Category { get; set; } = null!;
}
