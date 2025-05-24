using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Products.Queries.GetImagesById
{
    public class GetImagesByIdHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Product, int> repository
    ) : IQueryCollectionHandler<GetImagesByIdRequest, GetImagesByIdResponse>
    {
        public async Task<ResponseCollection<GetImagesByIdResponse>> Handle(
            GetImagesByIdRequest request,
            CancellationToken cancellationToken
        )
        {
            // Implementation goes here

            return new ResponseCollection<GetImagesByIdResponse>(
                IsSuccess: true,
                200,
                "",
                Data: []
            );
        }
    }
}
