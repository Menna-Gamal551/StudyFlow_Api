using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyFlow_Api.Data;
using StudyFlow_Api.Data.DTOs;
using StudyFlow_Api.Data.Models;
using StudyFlow_Api.Services.IService;
using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace StudyFlow_Api.Services
{
    public class AuthService(AppdbContext db,IMapper mapper,IPasswordHasher<User> passwordHasher) : IAuthService
    {
        private readonly AppdbContext _db=db;
        private readonly IMapper _mapper=mapper;
        private readonly IPasswordHasher<User> _passwordHasher= passwordHasher;
        public async Task<bool> IsUserNameExistAsync(string userName)
        {
            return await _db.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<UserDTO?> RegisterAsync(RegisterRequestDTO registerRequestDTO)
        {
           
                if (registerRequestDTO==null)
                {
                    throw new InvalidOperationException("Data is required");
                }
                if (await IsUserNameExistAsync(registerRequestDTO.UserName))
                {
                    throw new InvalidOperationException($"UserName with {registerRequestDTO.UserName} is already exsit!");
                }

                var user = new User
                {
                    Name = registerRequestDTO.Name,
                    UserName = registerRequestDTO.UserName,
                    CreatedAt = DateTime.UtcNow,
                };
                user.PasswordHash=_passwordHasher.HashPassword(user, registerRequestDTO.Password);
                _db.Users.Add(user);
              await _db.SaveChangesAsync();
                var userDto = _mapper.Map<UserDTO>(user);
                return userDto;
           
            }

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            if (loginRequestDTO==null)
                return null;
            var user = await _db.Users.FirstOrDefaultAsync(e => e.UserName==loginRequestDTO.UserName);
            if (user==null)
                return null;
            
          var result=  _passwordHasher.VerifyHashedPassword(user,user.PasswordHash,loginRequestDTO.Password);
            if (result==PasswordVerificationResult.Failed)
                return null;

            var userDto=_mapper.Map<UserDTO>(user);
            return new LoginResponseDTO
            {
                userDTO=userDto,
                token=string.Empty
            };
           
        }
    }
    }

