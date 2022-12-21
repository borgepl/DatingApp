using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;
        public LikesRepository(DataContext context)
        {
            this.context = context;
            
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await this.context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users = this.context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = this.context.Likes.AsQueryable();

            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.TargetUser);
            }

            if (likesParams.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargetUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }

           var likedUsers = users.Select(user => new LikeDto
            {
                Username = user.UserName,
                Id = user.Id,
                KnownAs = user.KnownAs,
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Age = user.DateOfBirth.Age()
            });

            return await PagedList<LikeDto>
                            .CreateAsync(likedUsers, likesParams.PageNumber, likesParams.PageSize); 

        }

        public async Task<AppUser> GetUserWithLikes(int UserId)
        {
            return await this.context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == UserId);
        }
    }
}