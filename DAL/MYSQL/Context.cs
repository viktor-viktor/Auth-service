using Microsoft.EntityFrameworkCore;

namespace AuthService.DAL.MYSQL
{
    public class MySqlContext: DbContext
    {
        public MySqlContext(DbContextOptions<MySqlContext> options) : base(options) { }
        public DbSet<User> _users { get; set; }
    }
}
