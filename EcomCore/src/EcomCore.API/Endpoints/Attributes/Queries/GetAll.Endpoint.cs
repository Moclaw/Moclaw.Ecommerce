using EcomCore.Application.Features.Attributes.Queries.GetAll;

namespace EcomCore.API.Endpoints.Attributes.Queries
{
    public class GetAllEndpoint(IMediator mediator)
        : CollectionEndpointBase<GetAllRequest, GetAllResponse>(mediator)
    {
        [HttpGet("attributes")]
        public override async Task<ResponseCollection<GetAllResponse>> HandleAsync(
            GetAllRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
