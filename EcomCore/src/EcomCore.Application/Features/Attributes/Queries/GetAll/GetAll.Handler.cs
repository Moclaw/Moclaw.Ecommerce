using Microsoft.Extensions.DependencyInjection;
using Shared.Utils;

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
            var attributes = await repository.GetAllAsync<GetAllResponse>(
                paging: new Pagination(default, request.PageIndex, request.PageSize),
                cancellationToken: cancellationToken
            );

            return new ResponseCollection<GetAllResponse>(
                IsSuccess: true, 
                200, 
                "Attributes retrieved successfully.",
                Data: [.. attributes.Entities]
            );
        }
    }
}
