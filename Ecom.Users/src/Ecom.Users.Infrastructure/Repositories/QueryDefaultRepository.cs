using EfCore.Repositories;
using Ecom.Users.Infrastructure.Persistence.EfCore;
using Shared.Entities;

namespace Ecom.Users.Infrastructure.Repositories;
public class QueryDefaultRepository<TEntity, TKey>(ApplicationDbContext dbContext)
    : QueryRepository<TEntity, TKey>(dbContext)
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}
