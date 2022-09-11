namespace HotelListing.API.Core.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        //base contains exception message
        public NotFoundException(string name, object key) : base($"{name} with id ({key}) was not found")
        {

        }
    }
}
