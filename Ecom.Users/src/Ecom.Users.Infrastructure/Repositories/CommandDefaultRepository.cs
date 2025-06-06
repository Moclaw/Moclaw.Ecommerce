using EfCore.Repositories;
using Microsoft.Extensions.Logging;
using Ecom.Users.Infrastructure.Persistence.EfCore;

namespace Ecom.Users.Infrastructure.Repositories;

public class CommandDefaultRepository(ApplicationDbContext dbContext, ILogger<CommandDefaultRepository> logger)
    : CommandRepository(dbContext, logger)
{ }
