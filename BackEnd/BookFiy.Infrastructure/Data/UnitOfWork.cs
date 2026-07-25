using BookFiy.Application.Interfaces;
using BookFiy.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFiy.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _appDbContext;
        private IDbContextTransaction? _transaction;
        public UnitOfWork(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
          
        }

        public async Task BeginTransactionAsync()
        {
          _transaction=await _appDbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            await _appDbContext.SaveChangesAsync();
            await _transaction!.CommitAsync();
           
        }

        public async Task RollbackAsync()
        {
            if (_transaction!=null)
               await _transaction.RollbackAsync();
            
        }

        public async Task SaveChangesAsync()
        {
            await _appDbContext.SaveChangesAsync();
        }
    }
}
