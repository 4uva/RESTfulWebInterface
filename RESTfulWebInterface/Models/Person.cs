using System.ComponentModel.DataAnnotations;

namespace RESTfulWebInterface.Models
{
    public class Person
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string LastName { get; set; } = string.Empty;//почему тут пустая строка 
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string ZipCode { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public Color Color { get; set; }
    }
}
