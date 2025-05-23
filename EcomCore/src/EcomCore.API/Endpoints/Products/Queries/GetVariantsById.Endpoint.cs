using EcomCore.Application.Features.Products.Queries.GetVariantsById;

namespace EcomCore.API.Endpoints.Products.Queries
{
    [Route("products")]
    public class GetVariantsByIdEndpoint(IMediator mediator) : EndpointBase<GetVariantsByIdRequest, GetVariantsByIdResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetVariantsByIdResponse>> HandleAsync(GetVariantsByIdRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
