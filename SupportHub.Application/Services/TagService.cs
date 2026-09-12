using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

//This class is the only class which knows about the data access layer and the database,
//so it should be responsible for retrieving and manipulating tag data.

public class TagService : ITagService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<TagService> _logger;

    public TagService(IApplicationDbContext db, ILogger<TagService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _db.Tags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return tags.Select(TagDto.From).ToList();
    }

    public async Task<TagDto?> GetTagByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await _db.Tags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return tag is null ? null : TagDto.From(tag);
    }

    public async Task<bool> TagExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tags.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> TagNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalised = Normalise(name);

        return await _db.Tags.AnyAsync(t => t.Name == normalised, cancellationToken);
    }

    public async Task<TagDto> CreateTagAsync(TagCreateDto tagCreateDto, CancellationToken cancellationToken = default)
    {
        // Names are stored lower case, so the plain unique index gives us
        // case-insensitive uniqueness without any extra work.
        var tag = new Tag { Name = Normalise(tagCreateDto.Name) };

        _db.Tags.Add(tag);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created tag {TagId} ({TagName})", tag.Id, tag.Name);

        return TagDto.From(tag);
    }

    public async Task DeleteTagAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag is null)
        {
            return;
        }

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted tag {TagId}", id);
    }

    public async Task AttachTagToTicketAsync(int ticketId, int tagId, CancellationToken cancellationToken = default)
    {
        var alreadyAttached = await _db.TicketTags
            .AnyAsync(tt => tt.TicketId == ticketId && tt.TagId == tagId, cancellationToken);

        // "Make sure this tag is on this ticket." If it already is, there is nothing to do.
        if (alreadyAttached)
        {
            return;
        }

        _db.TicketTags.Add(new TicketTag { TicketId = ticketId, TagId = tagId });
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Attached tag {TagId} to ticket {TicketId}", tagId, ticketId);
    }

    public async Task DetachTagFromTicketAsync(int ticketId, int tagId, CancellationToken cancellationToken = default)
    {
        var ticketTag = await _db.TicketTags
            .FirstOrDefaultAsync(tt => tt.TicketId == ticketId && tt.TagId == tagId, cancellationToken);

        // Already gone is the outcome the caller asked for.
        if (ticketTag is null)
        {
            return;
        }

        _db.TicketTags.Remove(ticketTag);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Detached tag {TagId} from ticket {TicketId}", tagId, ticketId);
    }

    private static string Normalise(string name) => name.Trim().ToLowerInvariant();
}
