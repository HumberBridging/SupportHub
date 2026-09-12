using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

//This class is the only class which knows about the data access layer and the database,
//so it should be responsible for retrieving and manipulating customer data.

public class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IApplicationDbContext db, ILogger<CustomerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Email))
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Email))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TicketDto>> GetCustomerTicketsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tickets
            .AsNoTracking()
            .Where(t => t.CustomerId == id)
            .OrderBy(t => t.Id)
            .Select(t => new TicketDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.CustomerId,
                t.AssignedAgentId,
                t.CreatedAtUtc,
                t.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CustomerExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Customers.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default)
    {
        var trimmed = email.Trim();

        // The comparison runs in SQL, so it matches the unique index exactly.
        var query = _db.Customers.Where(c => c.Email == trimmed);

        // On update, the customer keeping its own email is not a duplicate.
        if (excludeCustomerId is not null)
        {
            query = query.Where(c => c.Id != excludeCustomerId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> CustomerHasTicketsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Tickets.AnyAsync(t => t.CustomerId == id, cancellationToken);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto customerCreateDto, CancellationToken cancellationToken = default)
    {
        var customer = new Customer
        {
            Name = customerCreateDto.Name.Trim(),
            Email = customerCreateDto.Email.Trim()
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created customer {CustomerId}", customer.Id);

        return new CustomerDto(customer.Id, customer.Name, customer.Email);
    }

    public async Task UpdateCustomerAsync(int id, CustomerUpdateDto customerUpdateDto, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            return;
        }

        customer.Name = customerUpdateDto.Name.Trim();
        customer.Email = customerUpdateDto.Email.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated customer {CustomerId}", id);
    }

    public async Task DeleteCustomerAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (customer is null)
        {
            return;
        }

        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted customer {CustomerId}", id);
    }
}
