using EcomCore.Application.Features.Attributes.Queries.GetAll;

namespace EcomCore.API.Endpoints.Attributes.Queries
{
    [Route("attributes")]
    public class GetAllEndpoint(IMediator mediator) : EndpointBase<GetAllRequest, GetAllResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetAllResponse>> HandleAsync(GetAllRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
