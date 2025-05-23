using EcomCore.Application.Features.Products.Queries.GetAll;

namespace EcomCore.API.Endpoints.Products.Queries
{
    [Route("products")]
    public class GetAllEnpoint(IMediator mediator) : EndpointBase<GetAllRequest, GetAllResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetAllResponse>> HandleAsync(GetAllRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
