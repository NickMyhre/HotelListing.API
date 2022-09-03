using HotelListing.API.Models.Hotel;

namespace HotelListing.API.Models.Country
{
    /// <summary>
    /// Dto class for controlling what data is returned from http requests for countries
    /// </summary>
    public class CountryDto : BaseCountryDto
    {
        public int Id { get; set; }
        public List<HotelDto> Hotels { get; set; }
    }
}
