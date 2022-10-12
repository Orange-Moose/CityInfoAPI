using CityInfo.API.Entities;
using CityInfo.API.Services;
using System.Collections;

namespace CityInfo.API.Interfaces
{
    public interface ICityInfoRepository
    {
        Task<IEnumerable<City>> GetCitiesAsync();

        Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? nameFilter, string? searchQuery, int pageNumber, int pageSize);

        Task<City?> GetCityAsync(int cityId, bool includePOIs); // return value City is made nullable to avoid throwing

        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);

        Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int poiId); // return value PointOfInterest is made nullable to avoid throwing

        Task<bool> CityExistsAsync(int cityId); 

        Task AddPOIforCityAsync(int cityId, PointOfInterest pointOfInterest);

        void DeletePointOfInterest(PointOfInterest pointOfInterest);

        Task<bool> SaveChangesAsync();
    }
}
