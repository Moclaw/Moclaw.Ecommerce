using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Products.Queries.GetVariantsById
{
    public class GetVariantsByIdHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Product, int> repository
    ) : IQueryCollectionHandler<GetVariantsByIdRequest, GetVariantsByIdResponse>
    {
        public async Task<ResponseCollection<GetVariantsByIdResponse>> Handle(
            GetVariantsByIdRequest request,
            CancellationToken cancellationToken
        )
        {
            // Implementation goes here

            return new ResponseCollection<GetVariantsByIdResponse>(
                IsSuccess: true,
                200,
                "",
                Data: []
            );
        }
    }
}
