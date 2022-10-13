using CityInfo.API.Models;

namespace CityInfo.API
{
    //Used for development before using database
    public class CitiesDataStore
    {
        public List<CityDto> Cities { get; set; } // placeholder for dummy data
        
        //public static CitiesDataStore Current { get; } = new CitiesDataStore(); // return populated dummy data by calling CitiesDataStore.Current
        public CitiesDataStore()
        {
            //Init dummy data
            Cities = new List<CityDto>()
            {
                new CityDto() {
                    Id = 1,
                    Name = "Vilnius",
                    Description = "The one with the howling wolf.",
                    PointsOfInterest = new List<PointOfInterestDto>() {
                        new PointOfInterestDto() {Id = 11, Name = "TV Tower"},
                        new PointOfInterestDto() {Id = 12, Name = "Old Town"}
                    }
                },
                new CityDto() { 
                    Id = 2,
                    Name = "Kaunas",
                    Description = "The one that was once a capital.",
                    PointsOfInterest = new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto() {Id = 21, Name = "IX fort"}
                    }
                },
                new CityDto() { 
                    Id = 3,
                    Name = "Klaipėda",
                    Description = "The one with the SGD terminal.",
                    PointsOfInterest= new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto() {Id = 31, Name = "Sea Museum"}
                    }    
                },
                new CityDto() {Id = 4,
                    Name = "Vilkiškis",
                    Description = "The one that no one heard of.",
                    PointsOfInterest = new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto() {Id = 41, Name = "Devils something", Description = "Big tree and a few large stones"},
                        new PointOfInterestDto() {Id = 42, Name = "Devils Lake", Description = "Just some lake"}
                    }
                }
            };
        }
    }
}
