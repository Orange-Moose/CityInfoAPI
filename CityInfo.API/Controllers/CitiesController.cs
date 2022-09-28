using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")] // default route for this controller
    public class CitiesController : ControllerBase
    {
        [HttpGet]
        public JsonResult GetCities()
        {
            return new JsonResult
                (
                new List<object>()
                    {
                        new {id = 1, name = "Vilnius" },
                        new {id = 2, name = "Kaunas"}
                    }
                );
        }

        
    }
}
