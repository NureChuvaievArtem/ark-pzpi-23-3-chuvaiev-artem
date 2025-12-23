using System.Linq.Expressions;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repository;

/// <summary>
/// Extension methods for IGenericRepository to provide convenient async methods
/// Lab 3: Added for easier CRUD operations in admin services
/// </summary>
public static class GenericRepositoryExtensions
{
    public static async Task<IEnumerable<T>> GetAllAsync<T>(
        this IGenericRepository<T> repository,
        string? includeProperties = null) where T : BaseEntity
    {
        var includes = ParseIncludes<T>(includeProperties);
        var result = await repository.GetListByConditionAsync(
            condition: null,
            includes: includes
        );
        
        return result.IsSuccess ? result.Value : Enumerable.Empty<T>();
    }
    
    public static async Task<IEnumerable<T>> GetAllAsync<T>(
        this IGenericRepository<T> repository,
        Expression<Func<T, bool>> filter,
        string? includeProperties = null) where T : BaseEntity
    {
        var includes = ParseIncludes<T>(includeProperties);
        var result = await repository.GetListByConditionAsync(
            condition: filter,
            includes: includes
        );
        
        return result.IsSuccess ? result.Value : Enumerable.Empty<T>();
    }
    
    public static async Task<T?> GetByIdAsync<T>(
        this IGenericRepository<T> repository,
        int id,
        string? includeProperties = null) where T : BaseEntity
    {
        var includes = ParseIncludes<T>(includeProperties);
        var result = await repository.GetSingleByConditionAsync(
            condition: x => x.Id == id,
            includes: includes
        );
        
        return result.IsSuccess ? result.Value : null;
    }
    
    public static async Task AddAsync<T>(
        this IGenericRepository<T> repository,
        T entity) where T : BaseEntity
    {
        await repository.AddAsync(entity);
    }
    
    public static async Task UpdateAsync<T>(
        this IGenericRepository<T> repository,
        T entity) where T : BaseEntity
    {
        await repository.UpdateAsync(entity);
    }
    
    public static async Task DeleteAsync<T>(
        this IGenericRepository<T> repository,
        int id) where T : BaseEntity
    {
        await repository.DeleteAsync(x => x.Id == id);
    }
    
    public static async Task SaveAsync<T>(
        this IGenericRepository<T> repository) where T : BaseEntity
    {
        await repository.SaveAsync();
    }
    
    private static IEnumerable<Func<IQueryable<T>, IQueryable<T>>>? ParseIncludes<T>(string? includeProperties) where T : BaseEntity
    {
        if (string.IsNullOrWhiteSpace(includeProperties))
            return null;
        
        var includes = new List<Func<IQueryable<T>, IQueryable<T>>>();
        foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var propertyName = property.Trim();
            includes.Add(query => query.Include(propertyName));
        }
        
        return includes;
    }
}

