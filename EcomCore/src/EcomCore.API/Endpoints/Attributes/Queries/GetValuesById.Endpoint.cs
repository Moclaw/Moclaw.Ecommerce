using EcomCore.Application.Features.Attributes.Queries.GetValuesById;

namespace EcomCore.API.Endpoints.Attributes.Queries
{
    public class GetValuesByIdEndpoint(IMediator mediator)
        : SingleEndpointBase<GetValuesByIdRequest, GetValuesByIdResponse>(mediator)
    {
        [HttpGet("attributes/values/{id}")]
        public override async Task<Response<GetValuesByIdResponse>> HandleAsync(
            GetValuesByIdRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
