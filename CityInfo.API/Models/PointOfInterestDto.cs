namespace CityInfo.API.Models
{
    /// <summary>
    /// A DTO for a single point of interest
    /// </summary>
    public class PointOfInterestDto
    {
        /// <summary>
        /// The id of the point of interest
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The name of the point of interest
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The decsription of the point of interest
        /// </summary>
        public string Description { get; set; } = string.Empty; // property with default value
    }
}
