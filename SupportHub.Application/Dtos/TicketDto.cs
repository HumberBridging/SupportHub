using SupportHub.Domain.Enums;

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
    DateTime? UpdatedAtUtc);
