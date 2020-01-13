using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

using AuthService.DAL;
using AuthService.Models;

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

            string token = CreateToken(m_username, userData.role);
            
            return new Token { token = token };
        }

        public Token RegisterUser()
        {
            string token = null;
            if (m_mongo.AddNewUer(m_username, m_password))
            {
                token = CreateToken(m_username);
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

        private string CreateToken(string username, Role role = null)
        {
            if (role == null)
            {
                role = Role.Admin;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Secret.secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(CustomClaimTypes.Name.Value, username),
                    new Claim(CustomClaimTypes.Role.Value, role.Value)
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

    class AuthorizationService
    {
        public AuthorizationService(ErrorHandler errorHandler) { m_errorHandler = errorHandler; }
        public void Init(string data)
        {
            var key = Encoding.ASCII.GetBytes(Secret.secret);
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = new JwtSecurityToken();

            try
            {
                tokenHandler.ValidateToken(data, validationParameters, out token);
            }
            catch (Exception e)
            {
                m_errorHandler.SetErrorData(new HttpResult(401, e.Message));
                return;
            }

            m_token = (JwtSecurityToken)(token);
            if (m_token.ValidTo < DateTime.UtcNow)
            {
                m_errorHandler.SetErrorData(new HttpResult(401, "Token has expired"));
                return;
            }
        }

        public bool IsAdminRole()
        {
            string role = GetClaim(CustomClaimTypes.Role.Value);
            if (role != null && role == Role.Admin.Value)
            {
                return true;
            }

            m_errorHandler.SetErrorData(new HttpResult(403, "You lack permissions !"));
            return false;
        }
        public bool IsDevRole()
        {
            string role = GetClaim(CustomClaimTypes.Role.Value);
            if (role != null && role == Role.Dev.Value || role == Role.Admin.Value)
            {
                return true;
            }

            m_errorHandler.SetErrorData(new HttpResult(403, "You lack permissions !"));
            return false;
        }
        public bool IsUserRole()
        {
            string role = GetClaim(CustomClaimTypes.Role.Value);
            if (role != null && role == Role.User.Value || role == Role.Dev.Value || role == Role.Admin.Value)
            {
                return true;
            }

            m_errorHandler.SetErrorData(new HttpResult(403, "You lack permissions !"));
            return false;
        }

        private string GetClaim(string name)
        {
            if (m_token == null)
            {
                m_errorHandler.SetErrorData(new HttpResult(500, "Token not authorized at 'GetClaim'  method. Call AuthenticationService.Init first"));
                return null;
            }

            foreach (Claim claim in m_token.Claims)
            {
                if (claim.Type == name)
                {
                    return claim.Value;
                }
            }

            return null;
        }

        private JwtSecurityToken m_token;
        private readonly ErrorHandler m_errorHandler;
    }
}
