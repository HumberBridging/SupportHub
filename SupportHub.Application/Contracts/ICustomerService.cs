using SupportHub.Application.Dtos;

namespace SupportHub.Application.Contracts;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default);

    Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TicketDto>> GetCustomerTicketsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> CustomerExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, int? excludeCustomerId = null, CancellationToken cancellationToken = default);

    Task<bool> CustomerHasTicketsAsync(int id, CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto customerCreateDto, CancellationToken cancellationToken = default);

    Task UpdateCustomerAsync(int id, CustomerUpdateDto customerUpdateDto, CancellationToken cancellationToken = default);

    Task DeleteCustomerAsync(int id, CancellationToken cancellationToken = default);
}
