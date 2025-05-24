using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Attributes.Queries.GetAll
{
    public class GetAllHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)]
            IQueryRepository<Domain.Entities.Attribute, int> repository
    ) : IQueryCollectionHandler<GetAllRequest, GetAllResponse>
    {
        public async Task<ResponseCollection<GetAllResponse>> Handle(
            GetAllRequest request,
            CancellationToken cancellationToken
        )
        {
            // Implementation goes here

            return new ResponseCollection<GetAllResponse>(IsSuccess: true, 200, "", Data: []);
        }
    }
}
