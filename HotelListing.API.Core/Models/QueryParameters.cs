namespace HotelListing.API.Core.Models
{
    //class to assist with paging
    public class QueryParameters
    {
        //default page size of 15
        //allows client to specify their desired page size
        private int _pageSize = 15;
        public int StartIndex { get; set; }
        public int PageNumber { get; set; }
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
    }
}
