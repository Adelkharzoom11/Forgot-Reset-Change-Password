using ForotpasswordResetChange.Data.Constants;
using ForotpasswordResetChange.Data.Dtos.Auth;
using ForotpasswordResetChange.Data.Dtos.General;
using ForotpasswordResetChange.Data.Interfaces;
using ForotpasswordResetChange.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ForotpasswordResetChange.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService( UserManager<ApplicationUser> userManager , IConfiguration configuration , RoleManager<IdentityRole> roleManager)
        {
            this._userManager = userManager;
            this._configuration = configuration;
            this._roleManager = roleManager;
        }

        public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = await _userManager.FindByEmailAsync(registerDto.Email );
            if (user is not null)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 409,
                    Message = "UserName Already Exists"
                };
            }

            ApplicationUser newUser = new()
            {
                Id = Guid.NewGuid().ToString(),
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email
            };

            var createUserResult = await _userManager.CreateAsync(newUser,registerDto.Password);

            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation failed because: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 400,
                    Message = errorString
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            //var roleResult = await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
            if (!roleResult.Succeeded)
            {
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 400,
                    Message = "User erroe in roles"
                };
            }

            return new GeneralServiceResponseDto
            {
                IsSucceed = true,
                StatusCode = 201,
                Message = "User Created Successfully"
            };


        }

        public async Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user with username
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
                return new LoginServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "User is not found in this email!"
                }; 

            // check password of user
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
                return new LoginServiceResponseDto()
                {
                    IsSucceed = false,
                    Message = "Email Or Password Error"
                };

            // Return Token and userInfo to front-end
            var newToken = GenerateToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);

            return new LoginServiceResponseDto()
            {
                IsSucceed = true,
                Message = "Login Successfully",
                NewToken = newToken,
                UserInfo = userInfo
            };
        }



        public async Task<ForgotRestChangeResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user is null)
            {
                return new ForgotRestChangeResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            Console.WriteLine("============================================================ ");
            Console.WriteLine("token : " + token);
            Console.WriteLine("============================================================ ");

            var resetLink = $"http://localhost:4200/reset-password?email={user.Email}&token={token}";

            var client = new RestClient("https://send.api.mailtrap.io/api/send");

            var request = new RestRequest()
            {
                Method = Method.Post,
                RequestFormat = DataFormat.Json
            };
            request.AddHeader("Authorization", "Bearer 9bbfe61591c18490fd6607baa2025938");
            request.AddJsonBody(new
            {
                from = new { email = "mailtrap@demomailtrap.com" },
                to = new[] { new { email = user.Email } },
                template_uuid = "a7d77fcf-d74b-431a-a138-fe6ac69f2eb5",
                template_variables = new { user_email = user.Email, pass_reset_link = resetLink }
            });

            var response = await client.ExecuteAsync(request); // استخدام ExecuteAsync بدلاً من Execute للانتظار على الاستجابة

            if (response.IsSuccessful)
            {
                return new ForgotRestChangeResponseDto
                {
                    Token = token,
                    IsSuccess = true,
                    Message = "Email sent with password reset link. Please check your email"
                };
            }
            else
            {
                return new ForgotRestChangeResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to send email"
                };
            }
        }



        //public async Task<ForgotRestChangeResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        //{
        //    var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        //    if (user == null)
        //    {
        //        return new ForgotRestChangeResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "User does not exist with this email"
        //        };
        //    }

        //    try
        //    {
        //        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        Console.WriteLine("============================================================ ");
        //        Console.WriteLine("token : " + token);
        //        Console.WriteLine("============================================================ ");

        //        var resetLink = $"http://localhost:4200/reset-password?email={user.Email}&token={token}";

        //        var client = new RestClient("https://send.api.mailtrap.io/api/send");

        //        var request = new RestRequest
        //        {
        //            Method = Method.Post,
        //            RequestFormat = DataFormat.Json
        //        };
        //        request.AddHeader("Authorization", "Bearer 9bbfe61591c18490fd6607baa2025938");
        //        request.AddJsonBody(new
        //        {
        //            from = new { email = "mailtrap@demomailtrap.com" },
        //            to = new[] { new { email = user.Email } },
        //            template_uuid = "a7d77fcf-d74b-431a-a138-fe6ac69f2eb5",
        //            template_variables = new { user_email = user.Email, pass_reset_link = resetLink }
        //        });

        //        var response = await client.ExecuteAsync(request);

        //        if (response.IsSuccessful)
        //        {
        //            return new ForgotRestChangeResponseDto
        //            {
        //                Token = token,
        //                IsSuccess = true,
        //                Message = "Email sent with password reset link. Please check your email"
        //            };
        //        }
        //        else
        //        {
        //            return new ForgotRestChangeResponseDto
        //            {
        //                IsSuccess = false,
        //                Message = "Failed to send email. Error: " + response.ErrorMessage
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ForgotRestChangeResponseDto
        //        {
        //            IsSuccess = false,
        //            Message = "An error occurred while processing your request."
        //        };
        //    }
        //}






        public async Task<ForgotRestChangeResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
             //resetPasswordDto.Token = WebUtility.UrlDecode(resetPasswordDto.Token);

            if (user is null)
            {
                return new ForgotRestChangeResponseDto
                {
                    IsSuccess = false,
                    Message = "User dos Not Exist With this Email"
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return new ForgotRestChangeResponseDto
                {
                    IsSuccess = true,
                    Message = "Password Reset Successfully"
                };
            }

            return new ForgotRestChangeResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
            };
        }

        public async Task<ForgotRestChangeResponseDto> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);
            if (user is null)
            {
                return new ForgotRestChangeResponseDto
                {
                    IsSuccess = false,
                    Message = "User dos Not Exist With this Password"
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return new ForgotRestChangeResponseDto
                {
                    IsSuccess = true,
                    Message = "Password Change Successfully"
                };
            }

            return new ForgotRestChangeResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
            };
        }














        //GenerateJWTTokenAsync
        private string GenerateToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII
            .GetBytes(_configuration.GetSection("JWTSetting").GetSection("securityKey").Value!);

            var roles = _userManager.GetRolesAsync(user).Result;

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new(JwtRegisteredClaimNames.Name, user.FullName ?? ""),
                new(JwtRegisteredClaimNames.NameId, user.Id ?? ""),
                new(JwtRegisteredClaimNames.Aud,
                _configuration.GetSection("JWTSetting").GetSection("validAudience").Value!),
                new(JwtRegisteredClaimNames.Iss, _configuration.GetSection("JWTSetting").GetSection("validIssuer").Value!)
            ];


            foreach (var role in roles)

            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);


        }

        private UserInfoResult GenerateUserInfoObject(ApplicationUser user, IEnumerable<string> Roles)
        {
            // Instead of this, You can use Automapper packages. But i don't want it in this project
            return new UserInfoResult()
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = Roles
            };
        }

    }
}
