using AutoMapper;
using CityInfo.API.Interfaces;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")] // default route for this controller
    public class CitiesController : ControllerBase // Asp.NET Core base class for MVC controllers
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;   // nuget AutoMapper container

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper) // Dependency injection using Interface not the implementation
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));    
        }
        [HttpGet]
        public async Task<IActionResult> GetCities() // Task == promise, ActionResult == res //ActionResult<IEnumerable<CityWithoutPOIsDTO>>
        {
            // results from DB of type Entity
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();

            // construct result we want to return to client (in this case we omit POI list)
            var result  = _mapper.Map<IEnumerable<CityWithoutPOIsDTO>>(cityEntities);

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
