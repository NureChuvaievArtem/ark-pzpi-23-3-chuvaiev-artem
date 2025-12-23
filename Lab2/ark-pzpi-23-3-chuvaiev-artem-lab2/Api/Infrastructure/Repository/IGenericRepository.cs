using System.Linq.Expressions;
using Api.Infrastructure.ResultPattern;
using Api.Models;

namespace Api.Infrastructure.Repository;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<Result<IEnumerable<T>>> GetListByConditionAsync(
        Expression<Func<T, bool>>? condition = null,
        OrderByOptions<T>? orderBy = null,
        IEnumerable<Func<IQueryable<T>, IQueryable<T>>>? includes = null,
        bool? isNoTracking = null,
        bool? isSplitQuery = null
    );

    Task<Result<T>> GetSingleByConditionAsync(
        Expression<Func<T, bool>>? condition = null,
        IEnumerable<Func<IQueryable<T>, IQueryable<T>>>? includes = null,
        bool? isNoTracking = null,
        bool? isSplitQuery = null
    );

    Task<Result<int>> AddAsync(T item);

    Task<Result> AddRangeAsync(IEnumerable<T> items);

    Task<Result> UpdateAsync(T item);

    Task<Result> DeleteAsync(Expression<Func<T, bool>> condition);
    
    public Task SaveAsync();
}