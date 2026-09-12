using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

//This class is the only class which knows about the data access layer and the database,
//so it should be responsible for retrieving and manipulating ticket comment data.

public class TicketCommentService : ITicketCommentService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<TicketCommentService> _logger;

    public TicketCommentService(IApplicationDbContext db, ILogger<TicketCommentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<TicketCommentDto>> GetCommentsAsync(int ticketId, CancellationToken cancellationToken = default)
    {
        var comments = await _db.TicketComments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);

        return comments.Select(TicketCommentDto.From).ToList();
    }

    public async Task<TicketCommentDto?> GetCommentAsync(int ticketId, int commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _db.TicketComments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TicketId == ticketId && c.Id == commentId, cancellationToken);

        return comment is null ? null : TicketCommentDto.From(comment);
    }

    public async Task<bool> CommentExistsAsync(int ticketId, int commentId, CancellationToken cancellationToken = default)
    {
        return await _db.TicketComments
            .AnyAsync(c => c.TicketId == ticketId && c.Id == commentId, cancellationToken);
    }

    public async Task<TicketCommentDto> CreateCommentAsync(int ticketId, TicketCommentCreateDto ticketCommentCreateDto, CancellationToken cancellationToken = default)
    {
        var comment = new TicketComment
        {
            TicketId = ticketId,
            Body = ticketCommentCreateDto.Body.Trim(),
            AuthorAgentId = ticketCommentCreateDto.AuthorAgentId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created comment {CommentId} on ticket {TicketId}", comment.Id, ticketId);

        return TicketCommentDto.From(comment);
    }

    public async Task DeleteCommentAsync(int ticketId, int commentId, CancellationToken cancellationToken = default)
    {
        var comment = await _db.TicketComments
            .FirstOrDefaultAsync(c => c.TicketId == ticketId && c.Id == commentId, cancellationToken);

        if (comment is null)
        {
            return;
        }

        _db.TicketComments.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted comment {CommentId} on ticket {TicketId}", commentId, ticketId);
    }
}
