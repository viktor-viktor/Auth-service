using System.ComponentModel.DataAnnotations;

namespace AuthService.DAL.MYSQL
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Psw { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
