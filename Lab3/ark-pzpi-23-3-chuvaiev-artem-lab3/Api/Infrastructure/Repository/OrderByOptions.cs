using System.Linq.Expressions;

namespace Api.Infrastructure.Repository;

public class OrderByOptions<T>
{
    public Expression<Func<T, object>> Expression { get; set; }
    
    public bool IsDescending { get; set; }
}