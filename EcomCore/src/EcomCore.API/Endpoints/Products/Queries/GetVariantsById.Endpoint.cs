using EcomCore.Application.Features.Products.Queries.GetVariantsById;

namespace EcomCore.API.Endpoints.Products.Queries
{
    public class GetVariantsByIdEndpoint(IMediator mediator)
        : CollectionEndpointBase<GetVariantsByIdRequest, GetVariantsByIdResponse>(mediator)
    {
        [HttpGet("products-variants")]
        public override async Task<ResponseCollection<GetVariantsByIdResponse>> HandleAsync(
            GetVariantsByIdRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
