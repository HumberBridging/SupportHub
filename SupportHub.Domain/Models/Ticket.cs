using SupportHub.Domain.Enums;

namespace SupportHub.Domain.Models;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // PHASE 3 TODO: implement the status machine.
    // Legal moves are Open -> InProgress -> Resolved -> Closed, one step at a time.
    // Everything else must be refused with 409 Conflict.
    // public bool CanTransitionTo(TicketStatus next) => ...
}
