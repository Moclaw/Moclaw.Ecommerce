using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Categories.Queries.GetAll
{
    public class GetAllHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Category, int> repository
    ) : IQueryCollectionRequest<GetAllRequest, GetAllResponse>
    {
        public async Task<ResponseCollection<GetAllResponse>> Handle(
            GetAllRequest request,
            CancellationToken cancellationToken
        )
        {

            return new ResponseCollection<GetAllResponse>(IsSuccess: true, 200, "", Data: []);
        }
    }
}
