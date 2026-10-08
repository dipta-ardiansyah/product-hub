using System.ComponentModel.DataAnnotations;
using MediatR;
using ProductHub.Application.Products.Dtos;

namespace ProductHub.Application.Products.Commands
{
    public class UpdateProductCommand : IRequest<ProductDto>
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be 2-200 characters.")]
        public string Name { get; init; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string Description { get; init; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Price must be between 0.01 and 999,999,999.99.")]
        public decimal Price { get; init; }
    }
}