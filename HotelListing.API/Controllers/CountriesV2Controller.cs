using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models.Country;
using HotelListing.API.Core.Exceptions;

namespace HotelListing.API.Controllers
{
    [Route("api/v{Version:apiVersion}/countries")]
    [ApiController]
    [ApiVersion("2.0")]
    //[Authorize]
    public class CountriesV2Controller : ControllerBase
    {
        //inject Automapper into controller
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesV2Controller> _logger;

        //initializes an instance of DbContext upon api request
        public CountriesV2Controller(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesV2Controller> logger)
        {
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
            this._logger = logger;
        }

        // GET: api/Countries
        [HttpGet]
        [EnableQuery]  //enables OData functionality from this specific endpoint e.g. https://localhost:7048/api/v2/Countries/?$select=name only returns the name of a country
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            //return all countries
            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<List<GetCountryDto>>(countries);
            return Ok(records);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                //throw custom exception, this gets passed to exceptionmiddleware which generates the http response with error message + logging
                throw new NotFoundException(nameof(GetCountry), id); 
            }

            //map the country to the countryDto
            var countryDto = _mapper.Map<CountryDto>(country);

            return Ok(countryDto);
        }

        // PUT: api/Countries/5
        //takes id to verify update object exists, takes entire object because it updates the entire object
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                return BadRequest("Invalid Record Id");
            }

            //changes entity state to modified so that EF knows to update the entry
            //_context.Entry(country).State = EntityState.Modified;

            //get the record to be updated
            var country = await _countriesRepository.GetAsync(id);

            if (country == null)
            {
                //throw custom exception, this gets passed to exceptionmiddleware which generates the http response with error message + logging
                throw new NotFoundException(nameof(PutCountry), id);
            }
            
            //map put request to retrieved country, entity state automatically changed to modified
            _mapper.Map(updateCountryDto, country);

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            //200 response without content
            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountryDto)
        {
            //remap country to createCountryDto object
            var country = _mapper.Map<Country>(createCountryDto);

            await _countriesRepository.AddAsync(country);

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                //throw custom exception, this gets passed to exceptionmiddleware which generates the http response with error message + logging
                throw new NotFoundException(nameof(DeleteCountry), id);
            }

            await _countriesRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
