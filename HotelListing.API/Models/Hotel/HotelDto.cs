namespace HotelListing.API.Models.Hotel
{
    /// <summary>
    /// Dto class for controlling what data is returned from http requests for a list of hotels
    /// </summary>
    public class HotelDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Rating { get; set; }
        public int CountryId { get; set; }
    }
}
