using SupportHub.Application.Dtos;

namespace SupportHub.Application.Contracts;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(CancellationToken cancellationToken = default);

    Task<CustomerDto?> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default);
}
