using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Enums;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

//This class is the only class which knows about the data access layer and the database,
//so it should be responsible for retrieving and manipulating ticket data.

public class TicketService : ITicketService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<TicketService> _logger;

    public TicketService(IApplicationDbContext db, ILogger<TicketService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await _db.Tickets
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .ToListAsync(cancellationToken);

        // Mapped after ToListAsync so TicketDto.From runs in memory. Calling it inside
        // a Select would make EF try to translate it into SQL, and fail.
        return tickets.Select(TicketDto.From).ToList();
    }

    public async Task<TicketDto?> GetTicketByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return ticket is null ? null : TicketDto.From(ticket);
    }

    public async Task<bool> TicketExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tickets.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> AgentExistsAsync(int agentId, CancellationToken cancellationToken = default)
    {
        return await _db.Agents.AnyAsync(a => a.Id == agentId, cancellationToken);
    }

    public async Task<TicketStatus?> GetTicketStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return ticket?.Status;
    }

    public async Task<TicketDto> CreateTicketAsync(TicketCreateDto ticketCreateDto, CancellationToken cancellationToken = default)
    {
        // Both stamps share one timestamp so a brand-new ticket reads consistently.
        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            Title = ticketCreateDto.Title.Trim(),
            Description = ticketCreateDto.Description.Trim(),
            CustomerId = ticketCreateDto.CustomerId,
            AssignedAgentId = ticketCreateDto.AssignedAgentId,
            Priority = ticketCreateDto.Priority,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created ticket {TicketId} for customer {CustomerId}", ticket.Id, ticket.CustomerId);

        return TicketDto.From(ticket);
    }

    public async Task UpdateTicketAsync(int id, TicketUpdateDto ticketUpdateDto, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null)
        {
            return;
        }

        ticket.Title = ticketUpdateDto.Title.Trim();
        ticket.Description = ticketUpdateDto.Description.Trim();
        ticket.AssignedAgentId = ticketUpdateDto.AssignedAgentId;
        ticket.Priority = ticketUpdateDto.Priority;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated ticket {TicketId}", id);
    }

    public async Task<TicketDto?> TryChangeTicketStatusAsync(int id, TicketStatus nextStatus, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null)
        {
            return null;
        }

        // The rule lives on the entity, so it is the same rule everywhere.
        if (!ticket.CanTransitionTo(nextStatus))
        {
            _logger.LogInformation(
                "Refused status change on ticket {TicketId}: {CurrentStatus} -> {NextStatus}",
                id, ticket.Status, nextStatus);

            return null;
        }

        ticket.Status = nextStatus;
        ticket.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketId} moved to {NextStatus}", id, nextStatus);

        return TicketDto.From(ticket);
    }

    public async Task DeleteTicketAsync(int id, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null)
        {
            return;
        }

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted ticket {TicketId}", id);
    }
}
