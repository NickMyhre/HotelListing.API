namespace HotelListing.API.Core.Models
{
    public class PagedResult<T>
    {
        //number of records
        public int TotalCount { get; set; }
        //curent page
        public int PageNumber { get; set; }
        //record number
        public int RecordNumber { get; set; }
        //requested itmes
        public List<T> Items { get; set; }
    }
}
