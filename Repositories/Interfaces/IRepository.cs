// Repositories/Interfaces/IRepository.cs
using System.Linq.Expressions;

namespace Lab08_JeanLazarinos.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Remove(T entity);
    void Update(T entity); // Algunas implementaciones lo hacen async, pero EF lo maneja síncrono
}