using EcomCore.Application.Features.Attributes.Queries.GetAllAttributes;

namespace EcomCore.API.Endpoints.Attributes.Queries
{
    public class GetAllAttributesEndpoint(IMediator mediator)
        : CollectionEndpointBase<GetAllAttributesRequest, GetAllAttributesResponse>(mediator)
    {
        [HttpGet("attributes")]
        public override async Task<ResponseCollection<GetAllAttributesResponse>> HandleAsync(
            GetAllAttributesRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
