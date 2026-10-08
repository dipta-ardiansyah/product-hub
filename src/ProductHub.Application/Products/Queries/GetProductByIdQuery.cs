using System.ComponentModel.DataAnnotations;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Products.Dtos;

namespace ProductHub.Application.Products.Queries
{
    public class GetProductByIdQuery : ICacheableQuery<ProductDto>
    {
        [Required]
        public int Id { get; set; }

        public string CacheKey => $"item:{Id}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(5);

        public GetProductByIdQuery()
        {
        }

        public GetProductByIdQuery(int id)
        {
            Id = id;
        }
    }
}