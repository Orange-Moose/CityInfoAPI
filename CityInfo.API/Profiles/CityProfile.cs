using AutoMapper;

namespace CityInfo.API.Profiles
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            //create auto map from City entity to CityWithoutPOIsDTO
            CreateMap<Entities.City, Models.CityWithoutPOIsDTO>(); // Provide "From - To" sources. Eg. from /Entities/City.cs to /Models/CityWithoutPOIsDTO.cs
            CreateMap<Entities.City, Models.CityDto>();
        }
    }
}
