using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Products.Queries.GetReviewsById
{
    public class GetReviewsByIdHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Product, int> repository
    ) : IQueryCollectionHandler<GetReviewsByIdRequest, GetReviewsByIdResponse>
    {
        public async Task<ResponseCollection<GetReviewsByIdResponse>> Handle(
            GetReviewsByIdRequest request,
            CancellationToken cancellationToken
        )
        {
            // Implementation goes here

            return new ResponseCollection<GetReviewsByIdResponse>(
                IsSuccess: true,
                200,
                "",
                Data: []
            );
        }
    }
}
