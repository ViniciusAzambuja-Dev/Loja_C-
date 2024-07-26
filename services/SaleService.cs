using loja.data;
using loja.models;
using Microsoft.EntityFrameworkCore;

namespace loja.services
{
    public class SaleService
    {
        private readonly LojaDbContext _dbContext;

        public SaleService(LojaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task RecordSaleAsync(Venda sale)
        {
            
            var cliente = await _dbContext.Clientes.FindAsync(sale.ClienteId);
            if (cliente == null)
            {
                throw new ArgumentException($"Customer with ID {sale.ClienteId} not found.");
            }

           
            var produto = await _dbContext.Produtos.FindAsync(sale.ProdutoId);
            if (produto == null)
            {
                throw new ArgumentException($"Product with ID {sale.ProdutoId} not found.");
            }

           
            if (produto.QuantidadeDisponivel < sale.QuantidadeVendida)
            {
                throw new InvalidOperationException($"Insufficient quantity of product {produto.Nome} in stock.");
            }

           
            produto.QuantidadeDisponivel -= sale.QuantidadeVendida;

           
            _dbContext.Vendas.Add(sale);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<object>> GetSalesByProductDetailedAsync(int produtoId)
        {
            var sales = await _dbContext.Vendas
                .Where(v => v.ProdutoId == produtoId)
                .Include(v => v.Cliente)
                .Include(v => v.Produto)
                .Select(v => new
                {
                    v.Produto.Nome,
                    v.DataVenda,
                    v.Id,
                    ClienteNome = v.Cliente.Nome,
                    v.QuantidadeVendida,
                    v.PrecoVendaUnitario
                })
                .ToListAsync();

                if (sales.Count == 0)
                {
                    throw new InvalidOperationException($"No sales found for product with ID {produtoId}.");
                }

            return sales.Cast<object>().ToList();
        }

        public async Task<object> GetSalesByProductSummarizedAsync(int produtoId)
        {
            var summarized = await _dbContext.Vendas
                .Where(v => v.ProdutoId == produtoId)
                .GroupBy(v => v.Produto)
                .Select(g => new
                {
                    ProdutoNome = g.Key.Nome,
                    QuantidadeTotal = g.Sum(v => v.QuantidadeVendida),
                    RendimentoTotal = g.Sum(v => v.QuantidadeVendida * v.PrecoVendaUnitario)
                })
                .ToListAsync();

                 if (summarized.Count == 0)
                {
                    throw new InvalidOperationException($"No sales found for product with ID {produtoId}.");
                }

             return summarized.Cast<object>().ToList();
        }

        public async Task<List<object>> GetSalesByCustomerDetailedAsync(int clienteId)
        {
            var sales = await _dbContext.Vendas
                .Where(v => v.ClienteId == clienteId)
                .Include(v => v.Cliente)
                .Include(v => v.Produto)
                .Select(v => new
                {
                    v.Produto.Nome,
                    v.DataVenda,
                    v.Id,
                    ClienteNome = v.Cliente.Nome,
                    v.QuantidadeVendida,
                    v.PrecoVendaUnitario
                })
                .ToListAsync();

                if (sales.Count == 0)
                {
                    throw new InvalidOperationException($"No sales found for customer with ID {clienteId}.");
                }

            return sales.Cast<object>().ToList();
        }

        public async Task<List<object>> GetSalesByCustomerSummarizedAsync(int clienteId)
        {
            var summarized = await _dbContext.Vendas
            .Where(v => v.ClienteId == clienteId)
            .GroupBy(v => v.Produto)
            .Select(g => new
            {
                ProdutoNome= g.Key.Nome,
                QuantidadeTotal = g.Sum(v => v.QuantidadeVendida),
                RendimentoTotal = g.Sum(v => v.QuantidadeVendida * v.PrecoVendaUnitario)
            })
            .ToListAsync();

             if (summarized.Count == 0)
            {
                throw new InvalidOperationException($"No sales found for customer with ID {clienteId}.");
            }

            return summarized.Cast<object>().ToList();
        }
    }
}