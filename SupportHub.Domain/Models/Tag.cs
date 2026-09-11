namespace SupportHub.Domain.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// PHASE 4 TODO: the join entity. The composite key (TicketId, TagId) is what
// enforces "a ticket cannot have the same tag twice" — configure it in
// SupportHubContext.OnModelCreating.
