using EcomCore.Application.Features.Products.Queries.GetProductById;

namespace EcomCore.API.Endpoints.Products.Queries
{
    [Route("products")]
    public class GetProductByIdEndpoint(IMediator mediator) : EndpointBase<GetProductByIdRequest, GetProductByIdResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetProductByIdResponse>> HandleAsync(GetProductByIdRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
