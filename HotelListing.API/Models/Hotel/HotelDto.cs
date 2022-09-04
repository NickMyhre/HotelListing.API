namespace HotelListing.API.Models.Hotel
{
    /// <summary>
    /// Dto class for controlling what data is returned from http requests for a list of hotels
    /// </summary>
    public class HotelDto : BaseHotelDto
    {
        public int Id { get; set; }

    }
}
