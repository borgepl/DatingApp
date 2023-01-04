using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper  )
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) {
            
            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username already exists!");
            }
            // var user = this.mapper.Map<AppUser>(registerDto); --- Error on DateOnly and DateTime
            
            var user = new AppUser();

            user.KnownAs = registerDto.KnownAs;
            user.Gender =  registerDto.Gender;
            user.City = registerDto.City;
            user.Country = registerDto.Country;

            // Converting DateOnly to DateTime by providing Time Info
            DateOnly dateOnly = (DateOnly)registerDto.DateOfBirth;
            DateTime birthDateTime = dateOnly.ToDateTime(TimeOnly.Parse("10:00 PM"));

            user.DateOfBirth = birthDateTime;
            
            user.UserName = registerDto.Username.ToLower();
            
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);

            return new UserDto
            {
                username = user.UserName,
                token = await this.tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username) {
            return await this.userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) {
            
            var user = await this.userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.username);

            if (user == null) return Unauthorized("Invalid username");

            var result = await this.userManager.CheckPasswordAsync(user, loginDto.password);

            if (!result) return Unauthorized("Invalid password");

            return new UserDto
            {
                username = user.UserName,
                token = await this.tokenService.CreateToken(user),
                photoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

        }
    }
}