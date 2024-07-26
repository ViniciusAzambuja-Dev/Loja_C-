using Microsoft.EntityFrameworkCore;
using loja.data;
using loja.models;
namespace loja.services
{
    public class UserService
    {
        private readonly LojaDbContext _dbContext;
        public UserService(LojaDbContext dbContext)
        {
        _dbContext = dbContext;
        }

        public async Task<List<Usuario>> GetAllUsersAsync()
        {
            return await _dbContext.Usuarios.ToListAsync();
        }
  
        public async Task<Usuario> GetUserByIdAsync(int id)
        {
            return await _dbContext.Usuarios.FindAsync(id);
        }
    
        public async Task AddUserAsync(Usuario usuario)
        {
            _dbContext.Usuarios.Add(usuario);
            await _dbContext.SaveChangesAsync();
        }
       
        public async Task UpdateUserAsync(Usuario usuario)
        {
            _dbContext.Entry(usuario).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var usuario = await _dbContext.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _dbContext.Usuarios.Remove(usuario);
            await _dbContext.SaveChangesAsync();
        }
        }
    }
}