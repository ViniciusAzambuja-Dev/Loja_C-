using System.ComponentModel.DataAnnotations;

namespace loja.models
{
    public class Deposito
    {
        [Key]
        public int Id { get; set; }
        public string Nome { get; set; }
        
    }
}