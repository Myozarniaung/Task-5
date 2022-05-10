using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace cartapi.Models
{
    public class cartDBContext :DbContext
    {
        public cartDBContext(DbContextOptions<cartDBContext> options)
            : base(options)
        {
        }

        public DbSet<cart> carts { get; set; }
    }
}
