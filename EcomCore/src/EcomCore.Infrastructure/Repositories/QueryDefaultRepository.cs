using EfCore.Repositories;
using EcomCore.Infrastructure.Persistence.EfCore;
using Shared.Entities;

namespace EcomCore.Infrastructure.Repositories;
public class QueryDefaultRepository<TEntity, TKey>(ApplicationDbContext dbContext)
    : QueryRepository<TEntity, TKey>(dbContext)
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}
