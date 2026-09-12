using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupportHub.Application.Contracts;
using SupportHub.Application.Dtos;

namespace SupportHub.Api.Controllers;

[ApiController]
[Route("api/v1/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<CustomersController> _logger;
    private readonly ICustomerService _customerService;

    public CustomersController(ILogger<CustomersController> logger, ICustomerService customerService)
    {
        _logger = logger;
        _customerService = customerService;
    }

    //TODO: Add Paging and add a safeguard to limit the number of customers returned in a single request to avoid performance issues.
    //TODO: Add cancellation token to the method signature and pass it to the async methods to support request cancellation.
    [HttpGet]
    [ProducesResponseType<IEnumerable<CustomerDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retriving customers");

        // Implementation for getting customers
        var customers = await _customerService.GetAllCustomersAsync(cancellationToken);

        if(customers == null || !customers.Any())
        {
            _logger.LogInformation("No customers found");
            return NotFound();
        }

        return Ok(customers);
    }

    [HttpGet("{id:int}", Name = nameof(GetCustomerById))]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomerById(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving customer with ID {CustomerId}", id);
        
        // Implementation for getting a customer by ID
        var customer = await _customerService.GetCustomerByIdAsync(id, cancellationToken);
        
        if (customer == null)
        {
            _logger.LogInformation("Customer with ID {CustomerId} not found", id);
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerCreateDto customerCreateDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating a new customer");
        // Implementation for creating a new customer
        // This is a placeholder implementation. You would typically call a service method to create the customer.
        var newCustomer = new CustomerDto(3, customerCreateDto.Name, customerCreateDto.Email);
        // Return the created customer with a 201 Created response
        return CreatedAtRoute(nameof(GetCustomerById), new { id = newCustomer.Id }, newCustomer);
    }
}
