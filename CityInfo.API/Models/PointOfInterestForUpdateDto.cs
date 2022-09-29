using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models
{
    public class PointOfInterestForUpdateDto
    {
        [Required(ErrorMessage = "Please provide a Name")]
        [MaxLength(50,  ErrorMessage = "Maximum of 50 characters allowed")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150, ErrorMessage = "Maximum of 150 characters allowed")]
        public string? Description { get; set; }
    }
}
