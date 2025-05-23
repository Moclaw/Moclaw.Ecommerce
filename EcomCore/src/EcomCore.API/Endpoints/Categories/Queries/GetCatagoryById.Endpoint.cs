using EcomCore.Application.Features.Categories.Queries.GetCatagoryById;

namespace EcomCore.API.Endpoints.Categories.Queries
{
    [Route("categories")]
    public class GetCatagoryByIdEndpoint(IMediator mediator) : EndpointBase<GetCatagoryByIdRequest, GetCatagoryByIdResponse>(mediator)
    {
        [HttpGet]
        public async override Task<Response<GetCatagoryByIdResponse>> HandleAsync(GetCatagoryByIdRequest req, CancellationToken ct)
        {
            return await _mediator.Send(req, ct);
        }
    }
}
