using EcomCore.Application.Features.Attributes.DTOs;

namespace EcomCore.Application.Features.Products.Queries.GetVariantsById
{
    public class GetVariantsByIdResponse
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public List<AttributeDto>? Attributes { get; set; }
    }
}
