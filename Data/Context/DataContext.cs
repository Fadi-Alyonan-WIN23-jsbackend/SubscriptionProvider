using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<SubscriptionEntity> Subscriptions { get; set; }
}
