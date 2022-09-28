using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")] // default route for this controller
    public class CitiesController : ControllerBase // Asp.NET Core base class for MVC controllers
    {
        [HttpGet]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            var cities = CitiesDataStore.Current.Cities;

            return Ok(cities);
        }

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == id);


            if (city == null) return NotFound($"City with id: {id} was not found");

            return Ok(city); // return data + 200 OK status
        }

    }
}
