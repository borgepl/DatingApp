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

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;
        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper  )
        {
            this.mapper = mapper;
            this.tokenService = tokenService;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto) {
            
            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username already exists!");
            }
            // var user = this.mapper.Map<AppUser>(registerDto); --- Error on DateOnly and DateTime

            // add using to correctly dispose
            using var hmac = new HMACSHA512();
            
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
            // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            // user.PasswordSalt = hmac.Key;
            
            this.context.Users.Add(user);
            await this.context.SaveChangesAsync();

            return new UserDto
            {
                username = user.UserName,
                token = this.tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username) {
            return await this.context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto) {
            
            var user = await this.context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.username);
            if (user == null) return Unauthorized("Invalid username!");

          /*   using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.password));

            for (int i = 0; i < computedHash.Length; i++) {
                if (computedHash[i] != user.PasswordHash[i]) {
                    return Unauthorized("Invalid Password!");
                }
            } */

            return new UserDto
            {
                username = user.UserName,
                token = this.tokenService.CreateToken(user),
                photoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

        }
    }
}