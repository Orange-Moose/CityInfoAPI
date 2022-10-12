using AutoMapper;
using CityInfo.API.Interfaces;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace CityInfo.API.Controllers
{
    [Route("api/v{version:apiVersion}/cities/{cityId}/pointsofinterest")]
    [ApiController]
    [Authorize(Policy = "MustBeFromVilnius")] // comes from Program.cs authorization policy
    [ApiVersion("2.0")]
    public class PointsOfInterestController : ControllerBase // Asp.NET Core base class for MVC controllers
    {
        // Dependency injection (use interfaces, not instances)
        private readonly ILogger<PointsOfInterestController> _logger; // built-in .NET dependency
        private readonly IMailService _mailService; // custom dependency
        private readonly ICityInfoRepository _cityInfoRepository; // custom repo dependency 
        private readonly IMapper _mapper; // nuget dependency
        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // default dependency injection via constructor
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }





        [HttpGet] // route template is not necessary as we defined {cityId} variable in a default route for this controller
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            
            // Restricting route for users that are not from their home city

            //Using signed-in user info => extract Value of city in User entity
            var userCity = User.Claims.FirstOrDefault(c => c.Type == "city")?.Value;

            // compares if city with that name has that id
            if (!await _cityInfoRepository.CityNameMatchesCityId(userCity, cityId))
                return Forbid(); // returns 403 Unauthorized

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"The city id:{cityId} was not found when accessing points of interest");
                return NotFound();
            }

            var POIListforCity = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);

            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(POIListforCity));

        }

        [HttpGet("{poiid}", Name = "GetPointOfInterest")] // Name used in POST route, to construct and return result
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(int cityId, int poiId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId)) { return NotFound(); };

            var pointOfInterest = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, poiId);

            if (pointOfInterest == null) { return NotFound(); }

            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));

        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            //Checks if all validation annotations in passed DTO pass a validation. Eg [Required], [MaxLength(50)] etc.
            // This is done automatically by the [ApiController]
            //if(!ModelState.IsValid) { return BadRequest(); };

            var city = await _cityInfoRepository.GetCityAsync(cityId, true);
            if (city == null) { return NotFound(); };

            var pointOfInterestToAdd = _mapper.Map<Entities.PointOfInterest>(pointOfInterest); // returns object of type <Entities.PointOfInterest>

            // Add changes to DB context for saving 
            await _cityInfoRepository.AddPOIforCityAsync(cityId, pointOfInterestToAdd);

            //Perform changes added to the DB context
            await _cityInfoRepository.SaveChangesAsync();

            var createdPOItoReturn = _mapper.Map<PointOfInterestDto>(pointOfInterestToAdd);


            return CreatedAtRoute(
                "GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    poiId = createdPOItoReturn.Id // cityId and poiId are parameters used in GetPointOfInterest(int cityId, int poiId)
                },
                createdPOItoReturn // response body that includes created object
            ); // Returns 201 Created response
        }

        [HttpPut("{poiid}")] // updates all values of the resource. If some value were not provided they are set to default value
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int poiId, PointOfInterestForUpdateDto reqPointOfInterest)
        {
            var city = await _cityInfoRepository.GetCityAsync(cityId, false);
            if (city == null) { return NotFound(); };
            var poiEntityToUpdate = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, poiId);
            if (poiEntityToUpdate == null) { return NotFound(); };

            // Add changes to the DB context
            _mapper.Map(reqPointOfInterest, poiEntityToUpdate); // executes a mapping and overwriting values from input source object to existing object .Map(new, cur)

            // Perform changes added to the DB context
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent(); // returns 204 success status. We could also use 201 with CreatedAtRoute() containting the updated object
        }

        [HttpPatch("{poiId}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(int cityId, int poiId, JsonPatchDocument<PointOfInterestForUpdateDto> reqPointOfInterest)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
                return NotFound();

            var poiEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, poiId);
            if (poiEntity == null)
                return NotFound();

            var poiEntityToPatch = _mapper.Map<PointOfInterestForUpdateDto>(poiEntity);// converts entity to DTO for further modification

            // Validate POI Entity from DB againt pointOfInterest in a req.body, t.y. compare two DTOs
            reqPointOfInterest.ApplyTo(poiEntityToPatch, ModelState);


            //Checks if all validation annotations in passed DTO pass a validation. Eg [Required], [MaxLength(50)] etc.
            // This can't be done automatically by the [ApiController] because we are using a nugget that does not support this
            if (!ModelState.IsValid) 
                return BadRequest();

            // Validate copy of point of interest from DB againt a Model of DTO (defined in PointOfInterestForUpdateDto.cs)
            if (!TryValidateModel(poiEntityToPatch)) 
                return BadRequest(ModelState);

            // Add changes to the DB context
            _mapper.Map(poiEntityToPatch, poiEntity); // converts updated DTO back to Entity and overwrites values from DTO to entity object

            // Perform changes added to DB context
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent(); // returns 204 success status.
        }

        [HttpDelete("{poiId}")]
        public async Task<ActionResult> DeletePointOfInterest(int cityId, int poiId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId)) { return NotFound(); };
            
            var poi = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, poiId);
            if(poi == null) 
                return NotFound();

            // Add changes to the DB context
            _cityInfoRepository.DeletePointOfInterest(poi);

            // Perform changes added to DB context
            await _cityInfoRepository.SaveChangesAsync();

            _mailService.Send("POI was deleted", $"POI \"{poi.Name}\" was removed.");

            return NoContent(); // returns 204 success status.
        }
    }
}
