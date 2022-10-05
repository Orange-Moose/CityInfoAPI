using CityInfo.API.Entities;
using CityInfo.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.DbContexts
{
    public class CityInfoContext: DbContext
    {
        // Registering DB connection via constructor
        public CityInfoContext(DbContextOptions<CityInfoContext> options) : base(options) { } 
        
        
        public DbSet<City> Cities { get; set; } = null!; // null forgiving operator
        public DbSet<PointOfInterest> PointsOfInterest { get; set; } = null!; // null forgiving operator

        /*
        // Registering DB connection by overriding OnConfiguring() method
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("connectionstring");
            base.OnConfiguring(optionsBuilder);
        }
        */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Seeding DB with initial City dummy data
            modelBuilder.Entity<City>()
                .HasData(
                new City("Vilnius")
                {
                    Id = 1,
                    Description = "The one with the howling wolf.",
                },
                new City("Kaunas")
                {
                    Id = 2,
                    Description = "The one that was once a capital."
                },
                new City("Klaipeda") 
                {
                    Id = 3,
                    Description = "The one with the SGD terminal."
                }
                );

            //Seeding DB with initial POI dummy data
            modelBuilder.Entity<PointOfInterest>()
                .HasData(
                    new PointOfInterest("TV Tower")
                    {
                        Id = 11,
                        Description = "High spike with restaurant on top",
                        CityId = 1
                    },
                    new PointOfInterest("Old Town")
                    {
                        Id = 12,
                        CityId = 1
                    },
                    new PointOfInterest("IX Fort")
                    {
                        Id = 21,
                        CityId = 2
                    }, new PointOfInterest("Sea Museum")
                    {
                        Id = 31,
                        CityId = 3
                    }
                );
            base.OnModelCreating(modelBuilder);
        }
    }
}
