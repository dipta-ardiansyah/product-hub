using ProductHub.Domain.Entities;

namespace ProductHub.Application.Products.Dtos
{
    public record ProductDto(int Id, string Name, string Description, decimal Price, DateTime CreatedAt);

    public static class ProductMappings
    {
        public static ProductDto ToDto(this Product p) => new(p.Id, p.Name, p.Description, p.Price, p.CreatedAt);
    }
}