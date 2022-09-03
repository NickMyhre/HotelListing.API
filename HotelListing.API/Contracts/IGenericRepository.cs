namespace HotelListing.API.Contracts
{
    //represents a set of things to be implemented by a repository class(abstraction of a class)
    //this interface and associated repository class removes business logic from controllers
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id);
        Task<List<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> Exists(int id);
    }
}
