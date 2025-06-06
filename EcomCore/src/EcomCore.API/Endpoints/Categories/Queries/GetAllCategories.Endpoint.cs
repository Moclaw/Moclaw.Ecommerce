using EcomCore.Application.Features.Categories.Queries.GetAllCategories;

namespace EcomCore.API.Endpoints.Categories.Queries
{
    public class GetAllCategoriesEndpoint(IMediator mediator)
        : CollectionEndpointBase<GetAllCategoriesRequest, GetAllCategoriesResponse>(mediator)
    {
        [HttpGet("categories")]
        public override async Task<ResponseCollection<GetAllCategoriesResponse>> HandleAsync(
            GetAllCategoriesRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
