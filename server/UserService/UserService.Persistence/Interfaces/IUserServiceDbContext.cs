using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Persistence.Interfaces;

public interface IUserServiceDbContext
{
    DbSet<T> Set<T>() where T : class;
}