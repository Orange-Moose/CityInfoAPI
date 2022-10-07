using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using CityInfo.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext db; // DB context
        public CityInfoRepository(CityInfoContext dbContext) // injecting DB connection
        {
            db = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); // this is now points to connected db
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            var cities = await db.Cities.ToListAsync();
            return cities.OrderBy(x => x.Name);

            // or return db.Cities.OrderBy(x => x.Name).ToListAsync();

        }

        public async Task<City?> GetCityAsync(int cityId, bool includePOIs = false)
        {
            // Include POIs on demand using .Include() method
            if (includePOIs)
            {
                return await db.Cities.Where(c => c.Id == cityId).Include(x => x.PointsOfInterest).FirstOrDefaultAsync();
            }

            return await db.Cities.Where(c => c.Id == cityId).FirstOrDefaultAsync();

        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int poiId)
        {
            return await db.PointsOfInterest.Where(x => x.CityId == cityId && x.Id == poiId).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await db.PointsOfInterest.Where(x => x.CityId == cityId).ToListAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            var cities = await db.Cities.ToListAsync();
            return cities.Any(x => x.Id == cityId);

            // or return await db.Cities.AnyAsync(x => x.Id == cityId);
        }


        // Add changes to DB context for saving 
        public async Task AddPOIforCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false); // we don't need to load POIs as we simply add new one 
            if (city != null)
            {
                //this does not save POI to DB yet. The .Add method adds it to the object context but not to the DB
                // thats why this is not an async IO call (.AddAsync())
                // ...also the foreign key of new POI will be automatically set to the cityId
                city.PointsOfInterest.Add(pointOfInterest); 
            }
        }

        
        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            db.PointsOfInterest.Remove(pointOfInterest); // remove POI from PointsOfInterest table in DB
        }


        //Save changes added to the DB context
        public async Task<bool> SaveChangesAsync()
        {
            var numOfEntitiesChanged = await db.SaveChangesAsync(); 

            return (numOfEntitiesChanged >= 0); // returns true if 0 or more entities have been changed
        }
    }
}
