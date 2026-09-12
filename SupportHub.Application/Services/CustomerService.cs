using Microsoft.Extensions.Logging;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;

namespace SupportHub.Application.Services;

//This class is the only class which knows about the data access layer and the database,
//so it should be responsible for retrieving and manipulating customer data.

public class CustomerService : ICustomerService
{
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ILogger<CustomerService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
