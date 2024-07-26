using System.ComponentModel.DataAnnotations;

namespace loja.models
{
    public class Venda
    {
        [Key]
        public int Id { get; set; }
        public String DataVenda { get; set; }
        public String NumeroNotaFiscal { get; set; }
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }
        public int QuantidadeVendida { get; set; }
        public double PrecoVendaUnitario { get; set; }
    }
}