using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel;
using System.Security.Claims;

using CSharp.DAL;
using CSharp;

namespace CSharp
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
        public AuthenticationService(string username, string password)
        {
            m_username = username;
            m_password = password;
        }

        public string SignInUser()
        {
            string retVal = AuthServer.App.MongoDal.GetUserData(m_username, m_password);
            if (retVal == null)
            {
                throw new HttpBadRequestException("User with such credentials isn't found ! ");
            }

            string token = CreateToken(m_username);
            
            return token;
        }

        public string RegisterUser()
        {
            string answer = "User with such creds already exist";

            if (AuthServer.App.MongoDal.AddNewUer(m_username, m_password))
            {
                answer = CreateToken(m_username);
            }

            return answer;
        }

        public string UnregisterUser()
        {
            if (!AuthServer.App.MongoDal.RemoveUser(m_username, m_password))
            {
                throw new HttpBadRequestException("User with such credentials isn't found ! ");
            }

            return "User successfully removed !";
        }

        private string CreateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Secret.secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username)
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
    }

    class AuthorizationService
    {
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
                throw new HttpUnAuthorizedException(e.Message);
            }

            m_token = (JwtSecurityToken)(token);
            if (m_token.ValidTo < DateTime.UtcNow)
            {
                throw new HttpUnAuthorizedException("Token has expired");
            }
        }

        public string GetClaim(string name)
        {
            if (m_token == null)
            {
                throw new HttpInternalErrorException("Token not authorized at 'GetClaim'  method. Call AuthenticationService.Init first");
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
    }
}
