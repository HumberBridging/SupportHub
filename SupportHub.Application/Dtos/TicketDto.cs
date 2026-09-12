using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Enums;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Dtos;

public record TicketDto(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    int CustomerId,
    int? AssignedAgentId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc)
{
    // SQL Server's datetime2 does not store a DateTimeKind, so dates read back from
    // the database come out as Unspecified and serialise without the "Z". Stamping
    // them as UTC here keeps every response in the same shape.
    public static TicketDto From(Ticket ticket) => new(
        ticket.Id,
        ticket.Title,
        ticket.Description,
        ticket.Status,
        ticket.Priority,
        ticket.CustomerId,
        ticket.AssignedAgentId,
        DateTime.SpecifyKind(ticket.CreatedAtUtc, DateTimeKind.Utc),
        ticket.UpdatedAtUtc is null
            ? null
            : DateTime.SpecifyKind(ticket.UpdatedAtUtc.Value, DateTimeKind.Utc));
}

public class TicketCreateDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }

    public int? AssignedAgentId { get; set; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}

// Status is not here on purpose: it changes through POST /tickets/{id}/status,
// so a PUT cannot be used to skip steps in the status machine.
public class TicketUpdateDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    public int? AssignedAgentId { get; set; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}

public class TicketStatusUpdateDto
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus Status { get; set; }
}
