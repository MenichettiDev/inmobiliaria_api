using Microsoft.EntityFrameworkCore;
using inmobiliariaApi.Data;
using inmobiliariaApi.Models;

namespace inmobiliariaApi.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>
    {
        private readonly ILogger<RefreshTokenRepository> _logger;

        public RefreshTokenRepository(ApplicationDbContext context, ILogger<RefreshTokenRepository> logger)
            : base(context)
        {
            _logger = logger;
        }

        public async Task<RefreshToken?> GetActiveByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.Usuario)
                    .ThenInclude(u => u!.Rol)
                .FirstOrDefaultAsync(rt =>
                    rt.Token == token &&
                    rt.RevocadoEn == null &&
                    rt.ExpiraEn > DateTime.UtcNow);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeAllByUsuarioAsync(int idUsuario)
        {
            var tokens = await _dbSet
                .Where(rt => rt.IdUsuario == idUsuario && rt.RevocadoEn == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevocadoEn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredAsync()
        {
            var expired = await _dbSet
                .Where(rt => rt.ExpiraEn < DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            _dbSet.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }
}
