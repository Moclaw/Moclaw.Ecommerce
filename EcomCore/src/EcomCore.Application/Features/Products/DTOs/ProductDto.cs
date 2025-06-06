using EcomCore.Application.Features.Attributes.DTOs;
using EcomCore.Application.Features.Categories.DTOs;

namespace EcomCore.Application.Features.Products.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<CatagoryDto>? Categories { get; set; }
        public List<AttributeDto>? Attributes { get; set; }
    }
}
