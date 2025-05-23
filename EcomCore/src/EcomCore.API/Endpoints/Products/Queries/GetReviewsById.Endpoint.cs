using EcomCore.Application.Features.Products.Queries.GetReviewsById;

namespace EcomCore.API.Endpoints.Products.Queries
{
    [Route("products")]
    public class GetReviewsByIdEndpoint(IMediator mediator) : EndpointBase<GetReviewsByIdRequest, GetReviewsByIdResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetReviewsByIdResponse>> HandleAsync(GetReviewsByIdRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
