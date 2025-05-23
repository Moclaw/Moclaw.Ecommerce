using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Attributes.Queries.GetAll
{
    public class GetAllHandler
        (
         [FromKeyedServices(ServiceKeys.QueryRepository)]

            IQueryRepository<Domain.Entities.Attribute, int> repository
        )
        : IQueryCollectionHandler<GetAllRequest, GetAllResponse>
    {
        public async Task<Response<GetAllResponse>> Handle(GetAllRequest request, CancellationToken cancellationToken)
        {
            // Implementation goes here
            
            return new Response<GetAllResponse>(IsSuccess: true, 200, "", Data: new GetAllResponse());
        }
    }
}
