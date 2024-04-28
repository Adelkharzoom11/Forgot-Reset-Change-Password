using ForotpasswordResetChange.Data.Dtos.Auth;
using ForotpasswordResetChange.Data.Dtos.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForotpasswordResetChange.Data.Interfaces
{
    public interface IAuthService
    {
        public Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto regesterDto);
        public Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto);
        public Task<ForgotRestChangeResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordDto);
        public Task<ForgotRestChangeResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto);
        public Task<ForgotRestChangeResponseDto> ChangePassword(ChangePasswordDto changePasswordDto);

    }
}
