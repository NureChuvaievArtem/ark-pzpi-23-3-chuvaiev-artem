using System.Linq.Expressions;
using Api.Infrastructure.Data;
using Api.Infrastructure.ResultPattern;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    public readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    protected DbSet<T> Table => _context.Set<T>();

    public virtual async Task<Result<IEnumerable<T>>> GetListByConditionAsync(
        Expression<Func<T, bool>>? condition = null,
        OrderByOptions<T>? orderBy = null,
        IEnumerable<Func<IQueryable<T>, IQueryable<T>>>? includes = null,
        bool? isNoTracking = null,
        bool? isSplitQuery = null
    )
    {
        try
        {
            IQueryable<T> query = Table.AsQueryable();

            query = isNoTracking is not null && isNoTracking != false ? query.AsNoTracking() : query;
            query = isSplitQuery is not null && isSplitQuery != false ? query.AsSplitQuery() : query;

            if (condition is not null)
            {
                query = query.Where(condition);
            }

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            if (orderBy is not null)
            {
                query = orderBy.IsDescending
                    ? query.OrderByDescending(orderBy.Expression)
                    : query.OrderBy(orderBy.Expression);
            }

            var result = await query.ToListAsync();
            return Result<IEnumerable<T>>.Success(result);
        }
        catch (DbUpdateException ex)
        {
            return Result<IEnumerable<T>>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result<T>> GetSingleByConditionAsync(
        Expression<Func<T, bool>>? condition = null,
        IEnumerable<Func<IQueryable<T>, IQueryable<T>>>? includes = null,
        bool? isNoTracking = null,
        bool? isSplitQuery = null
    )
    {
        try
        {
            IQueryable<T> query = Table.AsQueryable();
            
            query = isNoTracking is not null && isNoTracking != false ? query.AsNoTracking() : query;
            query = isSplitQuery is not null && isSplitQuery != false ? query.AsSplitQuery() : query;
            
            if (condition is not null)
            {
                query = query.Where(condition);
            }
            
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            var result = await query.FirstOrDefaultAsync();

            if (result == null)
            {
                return Result<T>.Failure(RepositoryErrors<T>.NotFoundError);
            }

            return Result<T>.Success(result);
        }
        catch (DbUpdateException ex)
        {
            return Result<T>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result<int>> AddAsync(T item)
    {
        try
        {
            Table.Add(item);
            await SaveAsync();
            return Result<int>.Success(item.Id);
        }
        catch (DbUpdateException ex)
        {
            return Result<int>.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public virtual async Task<Result> AddRangeAsync(IEnumerable<T> items)
    {
        try
        {
            Table.AddRange(items);
            await SaveAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(RepositoryErrors<T>.AddError);
        }
    }

    public virtual async Task<Result> UpdateAsync(T item)
    {
        try
        {
            Table.Update(item);
            await SaveAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }

    public async Task<Result> DeleteAsync(Expression<Func<T, bool>> condition)
    {
        try
        {
            var deletedCount = await Table.Where(condition).ExecuteDeleteAsync();

            if (deletedCount == 0)
            {
                return Result.Failure(RepositoryErrors<T>.NotFoundError);
            }

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(RepositoryErrorMapper<T>.Map(ex));
        }
    }
    
    public async Task SaveAsync()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            var now = DateTime.UtcNow;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedOn = now;
            }

            entity.LastModifiedOn = now;
        }

        await _context.SaveChangesAsync();
    }
}