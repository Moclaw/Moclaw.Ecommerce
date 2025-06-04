using EcomCore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shared.Utils;

namespace EcomCore.Application.Features.Categories.Queries.GetAll
{
    public class GetAllHandler(
        [FromKeyedServices(ServiceKeys.QueryRepository)] IQueryRepository<Category, int> repository
    ) : IQueryCollectionHandler<GetAllRequest, GetAllResponse>
    {
        public async Task<ResponseCollection<GetAllResponse>> Handle(
            GetAllRequest request, 
            CancellationToken cancellationToken
        )
        {
            var categories = await repository.GetAllAsync<GetAllResponse>(
                paging: new Pagination(default, request.PageIndex, request.PageSize),
                cancellationToken: cancellationToken
            );

            return new ResponseCollection<GetAllResponse>(
                true,
                200,
                "Categories retrieved successfully.",
                [.. categories.Entities]
            );
        }
    }
}
