using System;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.IdentityModel.Tokens;

using AuthService.DAL;
using AuthService.Models;
using AuthService.Middleware;

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
    class AuthenticationService
    {
        public AuthenticationService(string username, string password, MongoDAL mongo, ErrorHandler errorHandler)
        {
            m_errorHandler = errorHandler;
            m_mongo = mongo;
            m_username = username;
            m_password = password;
        }

        public Token SignInUser()
        {
            User userData = m_mongo.GetUserData(m_username, m_password);
            if (userData == null)
            {
                m_errorHandler.SetErrorData(new HttpResult(400, "User with such credentials isn't found ! "));
                return null;
            }

            string token = CreateToken(userData);
            
            return new Token { token = token };
        }

        public Token RegisterUser(JsonElement data)
        {
            string token = null;
            User nUser = m_mongo.AddNewUer(m_username, m_password, data);
            if (nUser != null)
            {
                token = CreateToken(nUser);
            }
            else
            {
                m_errorHandler.SetErrorData(new HttpResult(400, "User with such creds already exist"));
                return null;
            }

            return new Token { token = token };
        }

        public string UnregisterUser()
        {
            if (!m_mongo.RemoveUser(m_username, m_password))
            {
                m_errorHandler.SetErrorData(new HttpResult(400, "User with such credentials isn't found ! "));
                return null;
            }

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
                    new Claim(CustomClaimTypes.Name.Value, user.name),
                    new Claim(CustomClaimTypes.Role.Value, user.role.Value),
                    new Claim(CustomClaimTypes.Custom.Value, user.Data.ToString())
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

        private string m_username;
        private string m_password;
        private readonly MongoDAL m_mongo;
        private readonly ErrorHandler m_errorHandler;
    }

}
