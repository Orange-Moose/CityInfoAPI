using CityInfo.API.Interfaces;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase // Asp.NET Core base class for MVC controllers
    {
        private readonly ILogger<PointsOfInterestController> _logger; // built-in .NET dependency on nuget dependency
        private readonly IMailService _mailService; // custom dependency
        private readonly CitiesDataStore _cityDataStore; // custom dependency
        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, CitiesDataStore cityDataStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // default dependency injection via constructor
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService)); // custom dependency injection via constructor
            _cityDataStore = cityDataStore ?? throw new ArgumentNullException(nameof(cityDataStore)); 
        }





        [HttpGet] // route template is not necessary as we defined {cityId} variable in a default route for this controller
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {

            try
            {
                // throw new Exception("Exeption sample"); // for demo purposes

                var city = _cityDataStore.Cities.Find(x => x.Id == cityId);
                if (city == null)
                {
                    _logger.LogInformation($"The city with id: {cityId} was not found when accessing points of interest.");
                    return NotFound();
                };

                return Ok(city.PointsOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exeption for getting points of interest for city  with id: {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }

        }

        [HttpGet("{poiid}", Name = "GetPointOfInterest")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int poiId)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null) { return NotFound(); };

            var PoI = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if (PoI == null) { return NotFound(); };

            return Ok(PoI);
        }

        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            //Checks if all validation annotations in passed DTO pass a validation. Eg [Required], [MaxLength(50)] etc.
            // This is done automatically by the [ApiController]
            //if(!ModelState.IsValid) { return BadRequest(); };

            var city = _cityDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (cityId == null) { return NotFound(); };

            var maxPointOfInterestId = _cityDataStore.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);

            var pointOfInterestToCreate = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(pointOfInterestToCreate);

            return CreatedAtRoute(
                "GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    poiId = pointOfInterestToCreate.Id // cityId and poiId are parameters used in GetPointOfInterest(int cityId, int poiId)
                },
                pointOfInterestToCreate
            ); // Returns 201 Created response
        }

        [HttpPut("{poiid}")] // updates all values of the resource. If some value were not provided they are set to default value
        public ActionResult UpdatePointOfInterest(int cityId, int poiId, PointOfInterestForUpdateDto pointOfInterest)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null) { return NotFound(); };

            var pointOfInterestToUpdate = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if (pointOfInterestToUpdate == null) { return NotFound(); };

            pointOfInterestToUpdate.Name = pointOfInterest.Name;
            pointOfInterestToUpdate.Description = pointOfInterest.Description;

            return NoContent(); // returns 204 success status. We could also use 201 with CreatedAtRoute() containting the updated object
        }

        [HttpPatch("{poiId}")]
        public ActionResult PartiallyUpdatePointOfInterest(int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> pointOfInterest)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null) { return NotFound(); };

            var cityPoi = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if (cityPoi == null) { return NotFound(); };

            // Create a copy if point of interest from DB
            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = cityPoi.Name,
                Description = cityPoi.Description,
            };

            // Validate copy of point of interest from DB againt pointOfInterest in a req.body
            pointOfInterest.ApplyTo(pointOfInterestToPatch, ModelState);


            //Checks if all validation annotations in passed DTO pass a validation. Eg [Required], [MaxLength(50)] etc.
            // This can't be done automatically by the [ApiController] because we are using a nugget that does not support this
            if (!ModelState.IsValid) { return BadRequest(); };

            // Validate copy if point of interest from DB againt a Model of DTO (defined in PointOfInterestForUpdateDto.cs)
            if (!TryValidateModel(pointOfInterestToPatch)) { return BadRequest(ModelState); }

            // Update DB item values with item values from req.body
            cityPoi.Name = pointOfInterestToPatch.Name;
            cityPoi.Description = pointOfInterestToPatch.Description;

            return NoContent(); // returns 204 success status.
        }

        [HttpDelete("{poiId}")]
        public ActionResult DeletePointOfInterest(int cityId, int poiId)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null) { return NotFound(); };

            var poi = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if (poi == null) { return NotFound(); };

            city.PointsOfInterest.Remove(poi);
            _mailService.Send("POI was deleted", $"POI \"{poi.Name}\" was removed from \"{city.Name}\".");

            return NoContent(); // returns 204 success status.
        }
    }
}
