using loja.data;
using loja.models;
using Microsoft.EntityFrameworkCore;
namespace loja.services
{
    public class StockService
    {
        private readonly LojaDbContext _dbContext;
        public StockService(LojaDbContext dbContext)
        {
        _dbContext = dbContext;
        }
        public async Task AddStockAsync(Deposito deposito)
        {
            _dbContext.Deposito.Add(deposito);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<object>> GetProductsInDepositoAsync(int depositoId)
        {
            var deposito = await _dbContext.Deposito.FindAsync(depositoId);

            if (deposito == null)
            {
                throw new ArgumentException($"Deposit with ID {depositoId} not found.");
            }

            var produtosSumarizados = await _dbContext.Produtos
            .Where(p => p.DepositoId == depositoId)
            .GroupBy(p => p.Nome)
            .Select(g => new
            {
                Nome = g.Key,
                Quantidade = g.Sum(p => p.QuantidadeDisponivel)
            })
            .ToListAsync<object>();

            return produtosSumarizados;
        }
           

       public async Task<object> GetProductQuantityInDepositoAsync(int produtoId)
        {
            var produto = await _dbContext.Produtos.FindAsync(produtoId);
            if (produto == null)
            {
                throw new ArgumentException($"Product with ID {produtoId} not found.");
            }

            return new
            {
                nome = produto.Nome,
                quantidade = produto.QuantidadeDisponivel
            };
        }
    }
}