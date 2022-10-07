using AutoMapper;

namespace CityInfo.API.Profiles
{
    public class PointsOfInterestProfile: Profile
    {
        public PointsOfInterestProfile()
        {
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>(); // create a map from POI Entity to POI DTO for getting a resource
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestForUpdateDto>(); // create a map from POI Entity to POI DTO for patching a resource
            CreateMap<Models.PointOfInterestForCreationDto, Entities.PointOfInterest>(); // create a map from POI DTO to POI Entity for saving a resource
            CreateMap<Models.PointOfInterestForUpdateDto, Entities.PointOfInterest>(); // create a map from POI DTO to POI Entity for updating a resource
        }
    }
}
