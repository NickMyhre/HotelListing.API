using System.ComponentModel.DataAnnotations.Schema;

namespace HotelListing.API.Data
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }

        //declares field is a foreign key---nameof(field) helps keep application more strongly typed
        [ForeignKey(nameof(CountryId))]
        public int CountryId { get; set; }

        //Reference to external entity required for foreign key
        public Country Country { get; set; }
    }
}
