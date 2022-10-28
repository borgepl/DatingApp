using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
            
        }

        [HttpGet]
        //[AllowAnonymous]
        // api/users
        // Could also use ActionResult<List<AppUser>> -- List of AppUser
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.userRepository.GetUsersAsync();
            var usersToReturn = this.mapper.Map<IEnumerable<MemberDto>>(users);
            
            return Ok(usersToReturn);

        }

       /*  [HttpGet("{id}")]
        // api/users/1
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            return Ok(await this.userRepository.GetUserByIdAsync(id));
            
        } */

        [HttpGet("{username}")]
        // api/users/lisa
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(username);

            return this.mapper.Map<MemberDto>(user);
            
        }
    }
}