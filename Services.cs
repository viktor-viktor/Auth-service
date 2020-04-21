using System;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;

using AuthService.DAL.MYSQL;

namespace AuthService
{
    class Secret
    {
        static public string secret
        {
            get { return value; }
        }
        private static string value = "bolshoi_i_moguchii_secret_takoi_shto_vse_vragi_boyatsa";
    }

    public class AuthenticationService
    {
        private MySqlDAL _sqlDal;
        private HttpRequest _request;
        private string _name;
        private string _psw;

        private void ParseAuthorizationHeader()
        {
            StringValues header;
            if (_request.Headers.TryGetValue("Authorization", out header))
            {
                string authHeader = header.ToString();
                authHeader = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string namePsw = encoding.GetString(Convert.FromBase64String(authHeader));

                int seperatorIndex = namePsw.IndexOf(':');
                _name = namePsw.Substring(0, seperatorIndex);
                _psw = namePsw.Substring(seperatorIndex + 1);
            }
        }

        public AuthenticationService(MySqlDAL sqlDal, IHttpContextAccessor contextAccessor)
        {
            _sqlDal = sqlDal;
            _request = contextAccessor.HttpContext.Request;
        }

        public async Task<string> SignInUser()
        {
            ParseAuthorizationHeader();
            User user = new User { Name = _name, Psw = _psw };
            if (! await _sqlDal.IsUserExist(user))
            {
                //throw exception
            }
            return CreateToken(user);
        }

        public async Task<string> RegisterUser(Role role = null)
        {
            if (role == null) role = Role.User;
            ParseAuthorizationHeader();

            User user = new User { Name = _name, Psw = _psw, Role = role.Value };

            if (! await _sqlDal.InsertNewUser(user))
            {
                //throw exceptions
            }

            return CreateToken(user);
        }

        public string UnregisterUser()
        {
            ParseAuthorizationHeader();
            User user = new User { Name = _name, Psw = _psw };
            _sqlDal.RemoveUser(user);

            return "User successfully removed !";
        }

        private string CreateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Secret.secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(CustomClaimTypes.Name.Value, user.Name),
                    new Claim(CustomClaimTypes.Role.Value, user.Role),
                    new Claim(CustomClaimTypes.UserId.Value, user.UserId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenVar = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(tokenVar);

            Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            var b65Token = Convert.ToBase64String(encoding.GetBytes(token));

            return token;
        }
    }

}
