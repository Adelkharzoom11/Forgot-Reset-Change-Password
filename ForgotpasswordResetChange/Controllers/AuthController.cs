using ForotpasswordResetChange.Data.Dtos.Auth;
using ForotpasswordResetChange.Data.Dtos.General;
using ForotpasswordResetChange.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ForgotpasswordResetChange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            this._authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            var registerResult = await _authService.RegisterAsync(registerDto);
            return Ok(registerResult);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginServiceResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var loginResult = await _authService.LoginAsync(loginDto);
            return Ok(loginResult);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotRestChangeResponseDto>> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _authService.ForgotPassword(forgotPasswordDto);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<ActionResult<ForgotRestChangeResponseDto>> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPassword(resetPasswordDto);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("change-password")]
        public async Task<ActionResult<ForgotRestChangeResponseDto>> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var result = await _authService.ChangePassword(changePasswordDto);
            return Ok(result);
        }


    }
}
