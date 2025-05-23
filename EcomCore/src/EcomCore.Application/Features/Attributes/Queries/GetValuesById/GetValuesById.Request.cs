using MinimalAPI.Attributes;

namespace EcomCore.Application.Features.Attributes.Queries.GetValuesById
{
    public class GetValuesByIdRequest : IQueryRequest<GetValuesByIdResponse>
    {
        public int Id { get; set; } 
    }
}
