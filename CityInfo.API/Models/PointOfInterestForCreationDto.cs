using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models
{
    public class PointOfInterestForCreationDto
    {
        [Required(ErrorMessage = "Please provide a Name value")] // Name field is required and returns a custom validation error message
        [MaxLength(50)]   //See default validation options in => System.ComponentModel.DataAnnotations.
        public string Name { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Description kas maximum of 150 characters")]
        public string? Description { get; set; }
    }
}
