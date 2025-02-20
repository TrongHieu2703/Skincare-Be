using Microsoft.AspNetCore.Mvc;
using Skincare.Services.Interfaces;
using Skincare.BusinessObjects.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly Skincare.Services.Interfaces.IAuthenticationService _authService;
        //nên sử dụng ILogger để log lỗi ở phía back end, tránh throw lỗi phức tạp ra phía client
        private ILogger<AuthenticationController> _logger;

        public AuthenticationController(Skincare.Services.Interfaces.IAuthenticationService authService, ILogger<AuthenticationController> logger)
        {
            _authService = authService;
            _logger = logger;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Skincare.BusinessObjects.DTOs.LoginRequest loginRequest)

        {
            //nên có try catch để hạn chế bị kill app khi găp lỗi bất ngờ
            try
            {
                var result = await _authService.LoginAsync(loginRequest);
                if (result == null)
                    return Unauthorized("Invalid email or password");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error login: " + ex);
                return BadRequest("An error has occured when login");
            }
            
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Skincare.BusinessObjects.DTOs.RegisterRequest registerRequest)
        {
            //nên có try catch để hạn chế bị kill app khi găp lỗi bất ngờ
            try
            {
                var result = await _authService.RegisterAsync(registerRequest);
                if (!result)
                    return BadRequest("Registration failed. Email might be in use.");

                return Ok("User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error register: " + ex);
                return BadRequest("An error has occured when register");
            }
        }  
    }
}
