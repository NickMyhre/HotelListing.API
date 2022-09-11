

using Microsoft.AspNetCore.Identity;

namespace HotelListing.API.Data
{
    //class to extend the default identity user class
    public class ApiUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
