namespace HotelListing.API.Data
{
    //table
    public class Country
    {
        //column set
        public int Id { get; set; }
        public  string Name { get; set; }
        public string ShortName { get; set; } //country code

        //need to let this class know if other classes reference it via foreign key

        //list of hotels due to many hotels in a country (one country to many hotels)
        public virtual IList<Hotel> Hotels { get; set; }

    }
}