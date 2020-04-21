using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using AuthService.Binding.Models;

namespace AuthService.Binding.Rest
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        public AuthenticationController(AuthenticationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<SignInRet> Get()
        {
            return new SignInRet { Token = await _service.SignInUser() };
        }


        [HttpPost]
        public async Task<SignInRet> Post([FromBody] UserData data = null)
        {
            if (data != null)
                data.ValidateData();

            return new SignInRet { Token = await _service.RegisterUser(Role.GerRoleFromString(data.Role)) };
        }

        [HttpDelete]
        public void Delete()
        {
            _service.UnregisterUser();
        }

        private readonly AuthenticationService _service;
    }

    [Route("api/authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        [Authorize(Policy = Policies.AdminOnly)]
        [HttpGet("admin")]
        public void GetAdmin()
        {
        }

        [Authorize(Policy = Policies.AdminAndDev)]
        [HttpGet("dev")]
        public void GetDev()
        {
        }

        [Authorize(Policy = Policies.All)]
        [HttpGet("public")]
        public void GetPublic()
        {
        }
    }
}
