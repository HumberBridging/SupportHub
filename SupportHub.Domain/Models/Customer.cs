namespace SupportHub.Domain.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Customer(int id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }
}
