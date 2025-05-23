using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdHandler
        (
         [FromKeyedServices(ServiceKeys.QueryRepository)]

            IQueryRepository<Product, int> repository
        )
        : IQueryHandler<GetProductByIdRequest, GetProductByIdResponse>
    {
        public async Task<Response<GetProductByIdResponse>> Handle(GetProductByIdRequest request, CancellationToken cancellationToken)
        {
            // Implementation goes here
            
            return new Response<GetProductByIdResponse>(IsSuccess: true, 200, "", Data: new GetProductByIdResponse());
        }
    }
}
