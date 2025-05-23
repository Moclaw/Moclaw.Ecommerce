using EcomCore.Application.Features.Attributes.Queries.GetValuesById;

namespace EcomCore.API.Endpoints.Attributes.Queries
{
    [Route("attributes")]
    public class GetValuesByIdEndpoint(IMediator mediator) : EndpointBase<GetValuesByIdRequest, GetValuesByIdResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetValuesByIdResponse>> HandleAsync(GetValuesByIdRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
