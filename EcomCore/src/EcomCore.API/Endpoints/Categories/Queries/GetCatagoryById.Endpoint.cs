using EcomCore.Application.Features.Categories.Queries.GetCatagoryById;

namespace EcomCore.API.Endpoints.Categories.Queries
{
    public class GetCatagoryByIdEndpoint(IMediator mediator)
        : SingleEndpointBase<GetCatagoryByIdRequest, GetCatagoryByIdResponse>(mediator)
    {
        [HttpGet("categories/{id}")]
        public override async Task<Response<GetCatagoryByIdResponse>> HandleAsync(
            GetCatagoryByIdRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
