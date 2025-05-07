namespace UserService.Application.Interfaces.ProductService;

public interface IProductServiceClient
{
    Task NotifyUserDeactivationAsync(Guid userId, CancellationToken cancellationToken);
}
