using HotelListing.API.Core.Models;

namespace HotelListing.API.Core.Contracts
{
    //represents a set of things to be implemented by a repository class(abstraction of a class)
    //this interface and associated repository class removes business logic from controllers
    public interface IGenericRepository<T> where T : class
    {
        //T functions take a generic entity and query and map afterwards
        //TResult functions are optimized implmentations that automap before executing the query
        Task<T> GetAsync(int? id);
        Task<TResult> GetAsync<TResult>(int? id);
        Task<List<T>> GetAllAsync();
        Task<List<TResult>> GetAllAsync<TResult>();
        Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParameters queryParameters);
        Task<T> AddAsync(T entity);
        Task<TResult> AddAsync<TSource, TResult>(TSource source);
        Task UpdateAsync(T entity);
        Task UpdateAsync<TSource>(int id, TSource source);
        Task DeleteAsync(int id);
        Task<bool> Exists(int id);
    }
}
