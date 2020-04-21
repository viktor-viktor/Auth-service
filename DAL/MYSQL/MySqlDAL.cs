using System.Linq;
using System.Buffers;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace AuthService.DAL.MYSQL
{
    public class MySqlDAL
    {
        private MySqlContext _context = null;
        private DbSet<User> _users = null;

        public MySqlDAL(MySqlContext context) { _context = context; _users = context._users; }

        public async Task<bool> InsertNewUser(User user)
        {
            if (await _users.Where<User>(u => u.Name == user.Name).CountAsync() != 0 )
            {
                return false;
            }

            _context.Add(user);
            _context.SaveChangesAsync();

            return true;
        }

        public void RemoveUser(User user)
        {
            _users.Remove(user);
            _context.SaveChanges();
        }

        public async Task<bool> IsUserExist(User user)
        {
            return await _users.Where<User>(u => u.Name == user.Name && u.Psw == user.Psw).CountAsync() != 0;
        }

        public async Task<User[]> GetAllUsers()
        {
            return await _users.ToArrayAsync<User>();
        }

        public void RepopulateUsers(User[] users)
        {
            // get users table name from _users
            string name = "USERS";
            // need extension setup for the following line
            //_context.Database. .ExecuteSqlCommand("TRUNCATE TABLE [TableName]");
        }
    }
}
