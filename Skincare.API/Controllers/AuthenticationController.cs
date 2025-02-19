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

        public AuthenticationController(Skincare.Services.Interfaces.IAuthenticationService authService)
        {
            _authService = authService;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Skincare.BusinessObjects.DTOs.LoginRequest loginRequest)

        {
            var result = await _authService.LoginAsync(loginRequest);
            if (result == null)
                return Unauthorized("Invalid email or password");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Skincare.BusinessObjects.DTOs.RegisterRequest registerRequest)
        {
            var result = await _authService.RegisterAsync(registerRequest);
            if (!result)
                return BadRequest("Registration failed. Email might be in use.");

            return Ok("User registered successfully");
        }
    }
}
