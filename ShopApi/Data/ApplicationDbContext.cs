using Microsoft.EntityFrameworkCore;
// using Shop.Models.Entities;
using ShopApi;

namespace Shop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Product> ProductsForApi { get; set; }
    }
}
