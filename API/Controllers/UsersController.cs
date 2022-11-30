using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.mapper = mapper;
            this.userRepository = userRepository;
            
        }

        [HttpGet]
        // api/users
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.userRepository.GetMembersAsync();
            
            return Ok(users);

        }

   /*      [HttpGet]
        //[AllowAnonymous]
        // api/users
        // Could also use ActionResult<List<AppUser>> -- List of AppUser
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.userRepository.GetUsersAsync();
            var usersToReturn = this.mapper.Map<IEnumerable<MemberDto>>(users);
            
            return Ok(usersToReturn);

        } */

       /*  [HttpGet("{id}")]
        // api/users/1
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            return Ok(await this.userRepository.GetUserByIdAsync(id));
            
        } */

        /* [HttpGet("{username}")]
        // api/users/lisa
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(username);

            return this.mapper.Map<MemberDto>(user);
            
        } */

        [HttpGet("{username}")]
        // api/users/lisa
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await this.userRepository.GetMemberAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) 
        {
            var username = User.GetUsername();
            var user = await this.userRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            this.mapper.Map(memberUpdateDto, user);

            if (await this.userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user!");
        } 

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
             var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

             if (user == null) return NotFound();

             var result = await this.photoService.AddPhotoAsync(file);

             if (result.Error != null) return BadRequest(result.Error.Message);

             var photo = new Photo 
             {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
             };

             if (user.Photos.Count == 0) photo.IsMain = true;

             user.Photos.Add(photo);

             // if (await this.userRepository.SaveAllAsync()) return mapper.Map<PhotoDto>(photo);

             if (await this.userRepository.SaveAllAsync()) 
             {
                return CreatedAtAction(nameof(GetUser), 
                            new {username = user.UserName}, mapper.Map<PhotoDto>(photo));
             }

             return BadRequest("Problem adding photo");
        }
    }
}