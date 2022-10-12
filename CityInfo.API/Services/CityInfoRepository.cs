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

        //Using tuple to return multiple vaues Task<(IEnumerable<City>, PaginationMetadata)>
        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? nameFilter, string? searchQuery, int pageNumber, int pageSize)
        {
            // Note: IEnumerable vs IQueryable interface
            // Deferred execution - read more...
            // IEnumerable - filter logic is executed on the client side (in-memory)
            // IQueriable - filter logic is executed on hte database side using SQL

            //Cast the Cities dbSet to IQueriable<City>
            var collection = db.Cities as IQueryable<City>;

            if (!string.IsNullOrEmpty(nameFilter))
            {
                // building a Filter query
                collection = collection.Where(x => x.Name == nameFilter.Trim()); 
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // building a Search query
                collection = collection.Where(x => x.Name.Contains(searchQuery.Trim()) || (x.Description != null && x.Description.Contains(searchQuery.Trim())));
            }

            // CountAsync() is a database method that returns total amount of items in a given collection
            var totalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber);


            // Pagination
            // database is not queried until you call .ToListAsync() method = defered execution
            var collectionToReturn =  await collection
                .OrderBy(x => x.Id)
                .Skip(pageSize * (pageNumber - 1)) // ignores the amount of results, eg. 5 items * (4 -1) pages = 15;
                .Take(pageSize) // returns x amount of items after previously skipped items, eg. if pageSize is 5, returns items from 16 to 20.
                .ToListAsync(); // database is not queried until you call .ToListAsync() method

            return (collectionToReturn, paginationMetadata);

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

        //Compares if city with that name, has that id
        public async Task<bool> CityNameMatchesCityId(string? cityName, int cityId)
        {
            return await db.Cities.AnyAsync(c => c.Id == cityId && c.Name == cityName);
        }


        //Save changes added to the DB context
        public async Task<bool> SaveChangesAsync()
        {
            var numOfEntitiesChanged = await db.SaveChangesAsync();

            return (numOfEntitiesChanged >= 0); // returns true if 0 or more entities have been changed
        }
    }
}
