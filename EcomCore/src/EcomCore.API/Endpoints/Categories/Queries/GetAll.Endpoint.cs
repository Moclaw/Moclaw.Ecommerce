using EcomCore.Application.Features.Categories.Queries.GetAll;

namespace EcomCore.API.Endpoints.Categories.Queries
{
    [Route("categories")]
    public class GetAllEndpoint(IMediator mediator) : EndpointBase<GetAllRequest, GetAllResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetAllResponse>> HandleAsync(GetAllRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
