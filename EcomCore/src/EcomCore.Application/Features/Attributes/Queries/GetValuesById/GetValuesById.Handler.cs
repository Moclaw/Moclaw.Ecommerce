using Microsoft.Extensions.DependencyInjection;

namespace EcomCore.Application.Features.Attributes.Queries.GetValuesById
{
    public class GetValuesByIdHandler
        (
         [FromKeyedServices(ServiceKeys.QueryRepository)]

            IQueryRepository<Domain.Entities.Attribute, int> repository
        )
        : IQueryHandler<GetValuesByIdRequest, GetValuesByIdResponse>
    {
        public async Task<Response<GetValuesByIdResponse>> Handle(GetValuesByIdRequest request, CancellationToken cancellationToken)
        {
            // Implementation goes here
            
            return new Response<GetValuesByIdResponse>(IsSuccess: true, 200, "", Data: new GetValuesByIdResponse());
        }
    }
}
