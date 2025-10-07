// Repositories/Repository.cs
using System.Linq.Expressions;
using Lab08_JeanLazarinos.Data;
using Lab08_JeanLazarinos.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lab08_JeanLazarinos.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly StoreLdbContext _context;

    public Repository(StoreLdbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
}