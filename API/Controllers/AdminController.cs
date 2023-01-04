using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        public AdminController( UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
            
        }

        // api/admin/users-with-roles
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            // return Ok("Only admins can see this");

            var users = await userManager.Users
                .Select(u => new 
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .OrderBy(u => u.Username)
                .ToListAsync();

            return Ok(users);
        }

         [Authorize(Policy = "RequireAdminRole")]
         [HttpPost("edit-roles/{username}")]
         public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
         {
            if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await userManager.FindByNameAsync(username);

            if (user == null) return NotFound();

            var existingUserRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(existingUserRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await userManager.RemoveFromRolesAsync(user, existingUserRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove existing roles");

            return Ok(await userManager.GetRolesAsync(user));

         }


        // api/admin/photos-to-moderate
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosForModeration()
        {
            return Ok("Admins or Moderators can see this");
        }
    }
}