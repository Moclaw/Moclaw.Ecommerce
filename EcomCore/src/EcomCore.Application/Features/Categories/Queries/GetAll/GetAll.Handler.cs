using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Categories.Queries.GetAll
{
    public class GetAllHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Category, int> repository
    ) : IQueryCollectionHandler<GetAllRequest, GetAllResponse>
    {
        public Task<ResponseCollection<GetAllResponse>> Handle(GetAllRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
