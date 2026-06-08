using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TD.Lib.Repository.Entity.Base;

namespace TD.Lib.Repository
{
    public interface ITDUnitOfWork : IDisposable
    {
        public ITDRepository<T> GetRepository<T>() where T : BaseEntity, new();
        public Task<int> SaveChangesAsync();
        public IDbContextTransaction BeginTransaction();
        public Task<IDbContextTransaction> BeginTransactionAsync();
        public IExecutionStrategy CreateExecutionStrategy();
    }

    public class TDUnitOfWork : ITDUnitOfWork
    {
        DbContext _dbContext;

        public TDUnitOfWork(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ITDRepository<T> GetRepository<T>() where T : BaseEntity, new()
        {
            return new TDRepository<T>(_dbContext);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.Collect();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public IExecutionStrategy CreateExecutionStrategy()
        {
            return _dbContext.Database.CreateExecutionStrategy();
        }
    }
}
