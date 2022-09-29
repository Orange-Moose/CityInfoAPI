using CityInfo.API.Models;
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
        [HttpGet] // route template is not necessary as we defined {cityId} variable in a default route for this controller
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.Find(x => x.Id == cityId);
            if (city == null) { return NotFound(); };

            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{poiid}", Name = "GetPointOfInterest")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int poiId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
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

            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (cityId == null) { return NotFound(); };

            var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(x => x.PointsOfInterest).Max(x => x.Id);

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
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if(city == null) { return NotFound(); };

            var pointOfInterestToUpdate = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if(pointOfInterestToUpdate == null) { return NotFound(); };

            pointOfInterestToUpdate.Name = pointOfInterest.Name;
            pointOfInterestToUpdate.Description = pointOfInterest.Description;

            return NoContent(); // returns 204 success status. We could also use 201 with CreatedAtRoute() containting the updated object
        }

        [HttpPatch("{poiId}")]
        public ActionResult PartiallyUpdatePointOfInterest(int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> pointOfInterest)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if(city == null) { return NotFound(); };

            var cityPoi = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if(cityPoi == null) { return NotFound(); };

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
            if(!TryValidateModel(pointOfInterestToPatch)) { return BadRequest(ModelState); }  

            // Update DB item values with item values from req.body
            cityPoi.Name = pointOfInterestToPatch.Name;
            cityPoi.Description = pointOfInterestToPatch.Description;

            return NoContent(); // returns 204 success status.
        }

        [HttpDelete("{poiId}")]
        public ActionResult DeletePointOfInterest(int cityId, int poiId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if(city == null) { return NotFound(); };    

            var poi = city.PointsOfInterest.FirstOrDefault(x => x.Id == poiId);
            if(poi == null) { return NotFound(); };

            city.PointsOfInterest.Remove(poi);

            return NoContent(); // returns 204 success status.
        }
    }
}
