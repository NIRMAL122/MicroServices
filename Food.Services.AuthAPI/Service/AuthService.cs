using Food.Services.AuthAPI.Data;
using Food.Services.AuthAPI.Models;
using Food.Services.AuthAPI.Models.Dto;
using Food.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace Food.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        public AuthService(AuthDbContext db,
                           UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = _db.applicationUsers.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                {
                    //create role if it doesn't exits
                    _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto requestDto)
        {
            var user = _db.applicationUsers.FirstOrDefault(u => u.UserName.ToLower() ==
                            requestDto.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, requestDto.Password);

            if(user == null || isValid == false)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }

            // if user was found, Generate JWT Token
            var token = _jwtTokenGenerator.GenerateToken(user);

            UserDto userDto = new()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            LoginResponseDto loginResponse = new LoginResponseDto()
            {
                User = userDto,
                Token = token,
            };
            return loginResponse;
        }

        public async Task<string> Register(RegisterRequestDto requestDto)
        {
            ApplicationUser user = new()
            {
                UserName = requestDto.Email,
                Email = requestDto.Email,
                NormalizedEmail = requestDto.Email,
                Name = requestDto.Name,
                PhoneNumber = requestDto.PhoneNumber
            };

            try
            {
                var result = await _userManager.CreateAsync(user, requestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.applicationUsers.First(
                                    u => u.UserName == requestDto.Email);

                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        ID = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber
                    };

                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch (Exception)
            {
                
            }
            return "Error Encountered";
        }
    }
}
