using System.ComponentModel.DataAnnotations;

namespace loja.models{
    public class Cliente{
        [Key]
        public int Id{get;set;}
        public String Nome{get;set;}
        public String Cpf{get;set;}
        public String Email{get;set;}
    }
}