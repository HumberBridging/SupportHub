namespace SupportHub.Domain.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<TicketTag> TicketTags { get; set; } = new List<TicketTag>();
}
