using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Booky.Models
{
    public class Category
    {
        public int Id { get; set; }
        [DisplayName("Catageory Name")]
        [Required]
        [MaxLength(20)]
        public string Name { get; set; } 
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order Number must be between 1 - 100")]
        public int DisplayOrder { get; set; }
    }
}
