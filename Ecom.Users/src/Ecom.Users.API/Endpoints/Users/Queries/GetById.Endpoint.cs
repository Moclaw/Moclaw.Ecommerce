using Ecom.Users.Application.Features.Users.Queries.GetById;

namespace Ecom.Users.API.Endpoints.Users.Queries
{
    public class GetByIdEndpoint(IMediator mediator)
        : SingleEndpointBase<GetByIdRequest, GetByIdResponse>(mediator)
    {
        [HttpGet("users/{id}")]
        public override async Task<Response<GetByIdResponse>> HandleAsync(
            GetByIdRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
