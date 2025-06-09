using Ecom.Core.Domain.Entities;
using Mapster;

namespace Ecom.Core.Application.Features.Products.Queries.GetAll;

public class GetAllMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Product, GetAllResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Images, src => src.Images.Select(img => img.ImageUrl).ToList())
            .Map(
                dest => dest.Categories,
                src => src.ProductCategories.Select(pc => pc.CategoryId).ToList()
            );
    }
}
