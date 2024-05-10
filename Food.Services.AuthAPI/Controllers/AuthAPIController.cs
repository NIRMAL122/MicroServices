using Food.Services.AuthAPI.Models.Dto;
using Food.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Food.Services.AuthAPI.Controllers
{
    [Route("api/Auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;

        public AuthAPIController(IAuthService authService)
        {
            _authService = authService;
            _response = new();
        }

        [HttpPost("register")]
        public  async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            var errorMsg = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                _response.IsSuccess = false;
                _response.Message = errorMsg;
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if(loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "UserName or Password is Incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegisterRequestDto model)
        {
            var assignRoleSuccessfully = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessfully)
            {
                _response.IsSuccess = false;
                _response.Message = "Error Encountered while Assigning Role";
                return BadRequest(_response);
            }
            return Ok(_response);
        }

    }
}
  