using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ErplaTangoDBContext : DbContext
    {
        public ErplaTangoDBContext(DbContextOptions<ErplaTangoDBContext> options) : base(options) {}
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
