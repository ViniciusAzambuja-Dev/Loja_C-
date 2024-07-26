using System.ComponentModel.DataAnnotations;

namespace loja.models{
    public class Usuario{
        [Key]
        public int Id { get; set;}
        public String Nome{get;set;}
        public String Email{get;set;}
        public String Senha{get;set;}
    }
}