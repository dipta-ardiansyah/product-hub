using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ProductHub.Application.Products.Commands
{
    public class DeleteProductCommand : IRequest<Unit>
    {
        [Required]
        public int Id { get; set; }

        public DeleteProductCommand()
        {
        }

        public DeleteProductCommand(int id)
        {
            Id = id;
        }
    }
}