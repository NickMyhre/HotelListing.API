using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Core.Models.Country
{
    /// <summary>
    /// Dto class for controlling what data is returned from http requests for a list of countries
    /// </summary>
    public class GetCountryDto : BaseCountryDto
    {
        public int Id { get; set; } 
    }
}
