using Microsoft.EntityFrameworkCore;
using BooksAPI.Models;  

namespace BooksAPI.Data
{
    public class ApiContext : DbContext
    {
        public virtual DbSet<Books> Books { get; set; }

        public ApiContext() { }
        public  ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {
                        
        }    
    }
}
