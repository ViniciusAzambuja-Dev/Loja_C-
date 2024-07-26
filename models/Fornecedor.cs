using System.ComponentModel.DataAnnotations;

namespace loja.models{

    public class Fornecedor{
        [Key]
        public int Id{get;set;}
        public String Cnpj{get;set;}
        public String Nome{get;set;}
        public String Endereço{get;set;}
        public String Email{get;set;}
        public String Telefone{get;set;}        

    }


}