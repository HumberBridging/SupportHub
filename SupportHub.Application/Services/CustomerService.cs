using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ILogger<CustomerService> logger)
    {
        _logger = logger;
    }

    //This class is the only class which knows about the data access layer and the database,
    //so it should be responsible for retrieving and manipulating customer data.
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving customers");
        
        return await Task.FromResult(new List<CustomerDto>
        {
            new CustomerDto(1, "John Doe", "john.doe@example.com"),
            new CustomerDto(2, "Jane Smith", "jane.smith@example.com")
        });
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var db = new List<Customer>
        {
            new Customer(1, "John Doe", "john.doe@example.com"),
            new Customer(2, "Jane Smith", "jane.smith@example.com")
        };

        var customer = db.FirstOrDefault(c => c.Id == id);

        //Conversion from Customer to CustomerDto
        var customerDto = customer != null ? new CustomerDto(customer.Id, customer.Name, customer.Email) : null;

        return await Task.FromResult(customerDto);
    }
}
