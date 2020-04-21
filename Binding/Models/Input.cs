
using AuthService;

namespace AuthService.Binding.Models
{
    public class UserData
    {
        public void ValidateData()
        {
            foreach (Role role in AuthService.Role.GetRolesList())
            {
                if (role.Value == Role) return;
            }

            //throw exception;
        }
        public string Role { get; set; }
    }
}
