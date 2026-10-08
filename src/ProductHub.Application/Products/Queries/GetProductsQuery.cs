using System.ComponentModel.DataAnnotations;
using ProductHub.Application.Common.Abstractions;
using ProductHub.Application.Common.Models;
using ProductHub.Application.Products.Dtos;

namespace ProductHub.Application.Products.Queries
{
    public class GetProductsQuery : ICacheableQuery<PagedResult<ProductDto>>, IValidatableObject
    {
        public string? Name { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "MinPrice must be non-negative.")]
        public decimal? MinPrice { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "MaxPrice must be non-negative.")]
        public decimal? MaxPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }
        public string? SortDir { get; set; }

        public string CacheKey =>
            $"list:{Name ?? ""}:{MinPrice?.ToString() ?? ""}:{MaxPrice?.ToString() ?? ""}:{Page}:{PageSize}:{SortBy ?? ""}:{SortDir ?? ""}";

        public TimeSpan? Expiration => TimeSpan.FromMinutes(2);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice.Value > MaxPrice.Value)
            {
                yield return new ValidationResult(
                    "MinPrice cannot be greater than MaxPrice.",
                    new[] { nameof(MinPrice), nameof(MaxPrice) });
            }
        }
    }
}