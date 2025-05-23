namespace EcomCore.Domain.Entities;

public class Attribute : BaseEntity, IEntity<int>
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
