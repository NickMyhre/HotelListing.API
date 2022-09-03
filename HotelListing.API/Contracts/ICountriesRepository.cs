using HotelListing.API.Data;

namespace HotelListing.API.Contracts
{
    //represents a set of things to be implemented by a repository class for interaction with countries
    //this interface and associated repository class removes business logic from controllers
    public interface ICountriesRepository : IGenericRepository<Country>
    {
        Task<Country> GetDetails(int id);
    }
}
