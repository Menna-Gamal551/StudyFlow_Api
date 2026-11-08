using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using StudyFlow_Api.Data.DTOs;
using StudyFlow_Api.Services.IService;

namespace StudyFlow_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService= authService;

        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>>Register(RegisterRequestDTO registerRequestDTO)
        { 
        if (registerRequestDTO == null) { return BadRequest(ApiResponse<object>.BadRequest("Data is required")); }
       var userDTO = await _authService.RegisterAsync(registerRequestDTO);
            if (userDTO==null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Registeration Failed"));
            }


            var response = ApiResponse<UserDTO>.CreatedAt("User registered Successfully", userDTO);
            return StatusCode(201, response);
        }
        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>>Login(LoginRequestDTO loginRequestDTO)
        {
            if (loginRequestDTO == null) { return BadRequest(ApiResponse<object>.BadRequest("Data is required")); }
            var response =await _authService.LoginAsync(loginRequestDTO);
            if(response==null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Login Failed please check your cardentials"));
            }
            return Ok(ApiResponse<LoginResponseDTO>.Ok("Login successed", response));
        }

    }

}
