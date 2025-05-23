using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shared.Utils;

namespace EcomCore.Application.Features.Products.Queries.GetAll
{
    public class GetAllHandler
        (
         [FromKeyedServices(ServiceKeys.QueryRepository)]

            IQueryRepository<Product, int> productRepository
        )
        : IQueryCollectionHandler<GetAllRequest, GetAllResponse>
    {
        public async Task<Response<GetAllResponse>> Handle(GetAllRequest request, CancellationToken cancellationToken)
        {
            var paging = new Paging(default, request.PageIndex, request.PageSize);

            var products = await productRepository.GetAllAsync<GetAllResponse>(
                predicate: p => string.IsNullOrEmpty(request.Search) || (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(request.Search) || !string.IsNullOrEmpty(p.Description) && p.Description.Contains(request.Search)),
                builder: q => q.OrderByDescending(p => p.CreatedAt),
                paging: paging,
                cancellationToken: cancellationToken);

            if (products == null || !products.Any())
            {
                return ResponseUtils.Error<GetAllResponse>(
                    code: 204,
                    message: "No products found."
                );
            }

            return ResponseUtils.Success(
                data: (GetAllResponse?)products,
                message: "Products retrieved successfully."
            );
        }
    }
}
