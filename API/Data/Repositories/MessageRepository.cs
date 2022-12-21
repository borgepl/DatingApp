using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;

namespace API.Data.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext context;
        public MessageRepository(DataContext context)
        {
            this.context = context;
            
        }
        public void AddMessage(Message message)
        {
            this.context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            this.context.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await this.context.Messages.FindAsync(id);
        }

        public Task<PagedList<MessageDto>> GetMessagesForUser()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MessageDto>> GetMessageThread(int currentUserId, int recipientId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync() > 0;
        }
    }
}