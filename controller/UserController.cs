using loja.data;

namespace loja.controllers
{
    public class UserController
    {
        private readonly LojaDbContext _dbContext;

        public UserController(LojaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool Login(string senha)
        {
            var usuario = _dbContext.Usuarios.FirstOrDefault(u => u.Senha == senha);

            if (usuario != null)
            {
                return true; 
            }
            else
            {
                return false; 
            }
        }
    }
}