using Api.Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api.Infrastructure.Repository;

public static class RepositoryErrorMapper<T>
{
    public static Error Map(DbUpdateException ex)
    {
        // Concurrency errors
        if (ex is DbUpdateConcurrencyException)
        {
            return RepositoryErrors<T>.UpdateError;
        }
        
        if (ex.InnerException is PostgresException pgEx)
        {
            switch (pgEx.SqlState)
            {
                // Unique constraints
                case "23505": // unique_violation
                    return RepositoryErrors<T>.AddError;
                // Foreign key constraints
                case "23503": // foreign_key_violation
                    if (pgEx.Message.Contains("DELETE", StringComparison.OrdinalIgnoreCase))
                    {
                        return RepositoryErrors<T>.DeleteError;
                    }
                    return RepositoryErrors<T>.UpdateError;
                default:
                    return RepositoryErrors<T>.UpdateError;
            }
        }

        return RepositoryErrors<T>.UpdateError;
    }
}