using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class PointOfInterest
    {
        public PointOfInterest(string name)
        {
            Name = name;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide a Name")]
        [MaxLength(50, ErrorMessage = "Maximum of 50 characters allowed")]
        public string Name { get; set; }

        [MaxLength(150, ErrorMessage = "Maximum of 150 characters allowed")]
        public string? Description { get; set; } = "Not provided";

        [ForeignKey("CityId")]
        public City? City { get; set; } // for creating a relationship between City and Point of interest in a DB. It will automatically target parent class Id [Key]

        public int CityId { get; set; } // define a foreign key
    }
}
