using Ecom.Users.Application.Features.Users.Queries.GetAll;

namespace Ecom.Users.API.Endpoints.Users.Queries
{
    public class GetAllEndpoint(IMediator mediator)
        : CollectionEndpointBase<GetAllRequest, GetAllResponse>(mediator)
    {
        [HttpGet("users")]
        public override async Task<ResponseCollection<GetAllResponse>> HandleAsync(
            GetAllRequest req,
            CancellationToken ct
        )
        {
            return await _mediator.Send(req, ct);
        }
    }
}
