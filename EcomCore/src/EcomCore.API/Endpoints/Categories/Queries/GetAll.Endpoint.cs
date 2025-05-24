using EcomCore.Application.Features.Categories.Queries.GetAll;

namespace EcomCore.API.Endpoints.Categories.Queries
{
    public class GetAllEndpoint(IMediator mediator)
        : CollectionEndpointBase<GetAllRequest, GetAllResponse>(mediator)
    {
        [HttpGet("categories")]
        public override async Task<ResponseCollection<GetAllResponse>> HandleAsync(
            GetAllRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
