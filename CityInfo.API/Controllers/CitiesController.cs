using AutoMapper;
using CityInfo.API.Interfaces;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")] // you can set multiple versions if needed
    [Route("api/v{version:apiVersion}/cities")] // default route for this controller
    public class CitiesController : ControllerBase // Asp.NET Core base class for MVC controllers
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;   // nuget AutoMapper container
        const int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper) // Dependency injection using Interface not the implementation
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        [HttpGet]
        public async Task<IActionResult> GetCities([FromQuery] string? nameFilter, [FromQuery] string? searchQuery, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) // Task == promise, ActionResult == res //ActionResult<IEnumerable<CityWithoutPOIsDTO>>
        {
            // Do not allow fetching more than "maxCitiesPageSize" pages at once
            if (pageSize > maxCitiesPageSize) pageSize = maxCitiesPageSize;

            var filterQuery = nameFilter;

            if (!string.IsNullOrEmpty(nameFilter))
            {
                // Capitalize first letter
                filterQuery = $"{nameFilter.ToCharArray()[0].ToString().ToUpper()}{nameFilter.Substring(1)}";
            }

            // Deconstruct result into two variables. Results from DB of type Tuple that contains IEnumerable of cities and Metadata object
            var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(filterQuery, searchQuery, pageNumber, pageSize);

            //Set custom header for paginationMetadata
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            // construct result we want to return to client (in this case we omit POI list)
            var result = _mapper.Map<IEnumerable<CityWithoutPOIsDTO>>(cityEntities);

            return Ok(result);
        }

        /// <summary>
        /// Get a city by id
        /// </summary>
        /// <param name="id"> The id of the city</param>
        /// <param name="includePOIs">Option to include points of interest</param>
        /// <returns>An IActionResult</returns>
        /// <response code="200">Returns a requested city</response>
        /// <response code="401">A valid token must be provided</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetCity(int id, bool includePOIs = false)
        {
            var cityEntity = await _cityInfoRepository.GetCityAsync(id, includePOIs);

            if (cityEntity == null) return NotFound($"City with id: {id} was not found");

            if (includePOIs)
            {
                return Ok(_mapper.Map<CityDto>(cityEntity));
            }

            return Ok(_mapper.Map<CityWithoutPOIsDTO>(cityEntity));
        }

    }
}
