using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Buoi6.Models;

namespace Buoi6.Models
{
    public class ProductImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "URL ảnh là bắt buộc")]
        public string ImageUrl { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product? Product { get; set; }
    }
}