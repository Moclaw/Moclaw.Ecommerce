using Ecom.Users.Infrastructure.Persistence.EfCore;
using EfCore.Repositories;
using Microsoft.Extensions.Logging;

namespace Ecom.Users.Infrastructure.Repositories;

public class CommandDefaultRepository(ApplicationDbContext dbContext, ILogger<CommandDefaultRepository> logger)
    : CommandRepository(dbContext, logger)
{ }
