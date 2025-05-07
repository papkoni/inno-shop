using System.Text;
using System.Text.Json;
using UserService.Application.Interfaces.ProductService;

namespace UserService.Infrastructure.ProductService;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task NotifyUserDeactivationAsync(Guid userId, CancellationToken cancellationToken)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(userId),
            Encoding.UTF8,
            "application/json");
        
        var response = await _httpClient.PatchAsync(
            $"products/availability", content, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
