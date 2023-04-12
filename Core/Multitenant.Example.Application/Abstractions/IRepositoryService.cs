using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitenant.Example.Application.Abstractions
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> XGetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveList(List<T> entity);
    }
}
