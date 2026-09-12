using Microsoft.EntityFrameworkCore;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Contracts;

public interface IApplicationDbContext
{
    public DbSet<Customer> Customers { get; }

    public DbSet<Agent> Agents { get; }
    public DbSet<Ticket> Tickets { get; }
    public DbSet<TicketComment> TicketComments { get; }
    public DbSet<Tag> Tags { get; }
    public DbSet<TicketTag> TicketTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
