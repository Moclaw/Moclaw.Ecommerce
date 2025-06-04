using MinimalAPI.Attributes;

namespace Ecom.Users.Application.Features.Users.Queries.GetById
{
    public class GetByIdRequest : IQueryRequest<GetByIdResponse>
    {
        [FromRoute]
        public Guid Id { get; set; }
    }
}
