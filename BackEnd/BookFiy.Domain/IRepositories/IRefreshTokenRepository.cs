using BookFiy.Domain.Entites;
using System;
using System.Threading.Tasks;

namespace BookFiy.Domain.IRepositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAsync(RefreshToken token);
    }
}
