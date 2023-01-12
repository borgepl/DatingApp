using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUnitOfWork uow;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        public UsersController(IUnitOfWork uow, IMapper mapper, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.mapper = mapper;
            this.uow = uow;
            
        }

        // Just for Tests with Postman // [Authorize(Roles = "Admin")]
        [HttpGet]
        // api/users
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            //var currentUser = await this.uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            var gender = await this.uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender)) {
                userParams.Gender = gender == "male" ? "female" : "male";
            }
            
            var users = await this.uow.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, 
                                            users.PageSize, users.TotalCount, users.TotalPages));
            
            return Ok(users);

        }

   /*      [HttpGet]
        //[AllowAnonymous]
        // api/users
        // Could also use ActionResult<List<AppUser>> -- List of AppUser
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.uow.UserRepository.GetUsersAsync();
            var usersToReturn = this.mapper.Map<IEnumerable<MemberDto>>(users);
            
            return Ok(usersToReturn);

        } */

       /*  [HttpGet("{id}")]
        // api/users/1
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            return Ok(await this.uow.UserRepository.GetUserByIdAsync(id));
            
        } */

        /* [HttpGet("{username}")]
        // api/users/lisa
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await this.uow.UserRepository.GetUserByUsernameAsync(username);

            return this.mapper.Map<MemberDto>(user);
            
        } */

        // Just to Tests // [Authorize(Roles = "Member")]
        [HttpGet("{username}")]
        // api/users/lisa
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await this.uow.UserRepository.GetMemberAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto) 
        {
            var username = User.GetUsername();
            var user = await this.uow.UserRepository.GetUserByUsernameAsync(username);

            if (user == null) return NotFound();

            this.mapper.Map(memberUpdateDto, user);

            if (await this.uow.Complete()) return NoContent();

            return BadRequest("Failed to update user!");
        } 

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
             var user = await this.uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

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

             if (await this.uow.Complete()) 
             {
                return CreatedAtAction(nameof(GetUser), 
                            new {username = user.UserName}, mapper.Map<PhotoDto>(photo));
             }

             return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId) 
        {
            var user = await this.uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("This is already your main photo!");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true; 

            if (await this.uow.Complete()) return NoContent();

            return BadRequest("Problem setting main photo");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> deletePhoto(int photoId) 
        {
            var user = await this.uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null)  return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete the main photo");

            if (photo.PublicId != null) {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await this.uow.Complete()) return Ok();

            return BadRequest("Problem deleting the photo");
        }
    }
}