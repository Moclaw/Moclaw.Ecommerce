using EcomCore.Application.Features.Products.Queries.GetImagesById;

namespace EcomCore.API.Endpoints.Products.Queries
{
    [Route("products")]
    public class GetImagesByIdEndpoint(IMediator mediator) : EndpointBase<GetImagesByIdRequest, GetImagesByIdResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetImagesByIdResponse>> HandleAsync(GetImagesByIdRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
