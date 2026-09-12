using System.ComponentModel.DataAnnotations;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Dtos;

public record TagDto(int Id, string Name)
{
    public static TagDto From(Tag tag) => new(tag.Id, tag.Name);
}

public class TagCreateDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
