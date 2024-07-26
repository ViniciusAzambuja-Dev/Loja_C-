using Microsoft.EntityFrameworkCore;
using loja.data;
using loja.models;
namespace loja.services
{
    public class ClientService
    {
        private readonly LojaDbContext _dbContext;
        public ClientService(LojaDbContext dbContext)
        {
            _dbContext = dbContext;
        }
     
        public async Task<List<Cliente>> GetAllClientsAsync()
        {
            return await _dbContext.Clientes.ToListAsync();
        }
      
        public async Task<Cliente> GetClientByIdAsync(int id)
        {
            return await _dbContext.Clientes.FindAsync(id);
        }
     
        public async Task AddClientAsync(Cliente cliente)
        {
            _dbContext.Clientes.Add(cliente);
            await _dbContext.SaveChangesAsync();
        }
     
        public async Task UpdateClientAsync(Cliente cliente)
        {
            _dbContext.Entry(cliente).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    
        public async Task DeleteClientAsync(int id)
        {
            var cliente = await _dbContext.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _dbContext.Clientes.Remove(cliente);
            await _dbContext.SaveChangesAsync();
        }
        }
    }
}