using Core.Domain.Base;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repositories
{
    public class TangoRespositoryBase<T> : IAsyncRepository<T> where T : BaseEntity
    {
        private readonly DbSet<T> _entitySet;
        public TangoRespositoryBase(ErplaTangoDBContext erplaTangoDB)
        {
            _entitySet = erplaTangoDB.Set<T>();
        }
        public async Task<T> AddAsync(T entity)
        {
            await _entitySet.AddAsync(entity);
            return entity;
        }

        public Task<bool> DeleteAsync(T entity)
        {
            _entitySet.Remove(entity);
            return Task.FromResult(true);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> expression)
        {
            return await _entitySet.FirstOrDefaultAsync(expression);
        }

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>> expression)
        {
            return await _entitySet.Where(expression).ToListAsync();
        }

        public Task<T> UpdateAsync(T entity)
        {
            _entitySet.Update(entity);
            return Task.FromResult(entity);
        }
    }
}
