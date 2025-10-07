// Repositories/Interfaces/IUnitOfWork.cs
using Lab08_JeanLazarinos.Models;

namespace Lab08_JeanLazarinos.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Aquí declaramos una propiedad por cada repositorio que necesitemos
    IRepository<Product> ProductRepository { get; }
    IRepository<Client> ClientRepository { get; }
    
    // ... agrega los otros repositorios que necesites (Order, OrderDetail)
    IRepository<Order> OrderRepository { get; }
    IRepository<Orderdetail> OrderDetailRepository { get; } 

    Task<int> CompleteAsync();
}