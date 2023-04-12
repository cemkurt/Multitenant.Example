using Multitenant.Example.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitenant.Example.Application.Abstractions
{
    public interface IProductService : IRepository<Product>
    {
        Task<Product> CreateAsync(string name, string description, int rate);
        Task<Product> GetByIdAsync(int id);
        Task<IReadOnlyList<Product>> GetAllAsnyc();
    }
}
