using EfCore.Repositories;
using Microsoft.Extensions.Logging;
using EcomCore.Infrastructure.Persistence.EfCore;

namespace EcomCore.Infrastructure.Repositories;

public class CommandDefaultRepository(ApplicationDbContext dbContext, ILogger<CommandDefaultRepository> logger)
    : CommandRepository(dbContext, logger)
{ }
