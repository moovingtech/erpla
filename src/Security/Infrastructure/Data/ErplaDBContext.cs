using Core.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ErplaDBContext : IdentityDbContext<User>  
    {  
        public ErplaDBContext(DbContextOptions<ErplaDBContext> options) : base(options)
            {

            }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }  
}
