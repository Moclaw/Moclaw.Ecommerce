using Ecom.Users.Infrastructure.Persistence.EfCore;
using EfCore.Repositories;
using Shared.Entities;

namespace Ecom.Users.Infrastructure.Repositories;
public class QueryDefaultRepository<TEntity, TKey>(ApplicationDbContext dbContext)
    : QueryRepository<TEntity, TKey>(dbContext)
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
}
