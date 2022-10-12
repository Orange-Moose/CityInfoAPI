using AutoMapper;
using CityInfo.API.Interfaces;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")] // default route for this controller
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
        public async Task<IActionResult> GetCities([FromQuery] string? nameFilter, [FromQuery] string? searchQuery, int pageNumber = 1, int pageSize = 10) // Task == promise, ActionResult == res //ActionResult<IEnumerable<CityWithoutPOIsDTO>>
        {
            // Do not allow fetching more than "maxCitiesPageSize" pages at once
            if (pageSize > maxCitiesPageSize) pageSize = maxCitiesPageSize;

            var filterQuery = nameFilter;
            // Capitalize first letter
            if (!string.IsNullOrEmpty(nameFilter))
            {
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

        [HttpGet("{id}")]
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
