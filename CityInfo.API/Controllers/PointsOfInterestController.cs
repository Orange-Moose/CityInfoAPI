using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase // Asp.NET Core base class for MVC controllers
    {
        [HttpGet] // route template is not necessary as we defined {cityId} variable in a default route for this controller
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.Find(x => x.Id == cityId);
            if(city == null) { return NotFound(); };

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{poiId}")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int poiId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if(city == null ) { return NotFound(); };

            var PoI = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if(PoI == null) { return NotFound(); };

            return Ok(PoI);
        }
    }
}
