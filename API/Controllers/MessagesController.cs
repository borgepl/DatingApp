using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork uow;
        public MessagesController(  ILogger<MessagesController> logger, 
                                    IUnitOfWork uow,
                                    IMapper mapper
                                )
        {
            _logger = logger;
            this.uow = uow;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto) 
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) {
                return BadRequest("You cannot send messages to yourself");
            }

            var sender = await uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            uow.MessageRepository.AddMessage(message);

            if (await uow.Complete()) return Ok(mapper.Map<MessageDto>(message));

            return BadRequest("Something went wrong in creating message!");

        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] 
            MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await uow.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(
                messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages ));
            
            return messages;
        }

      /*   [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerator<MessageDto>>> GetMessageThread(string username)
        {
            var currentUserName = User.GetUsername();

            return Ok(await uow.MessageRepository.GetMessageThread(currentUserName, username));
        } */

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var currentUserName = User.GetUsername();

            var message = await uow.MessageRepository.GetMessage(id);

            if (message.SenderUsername != currentUserName && message.RecipientUsername != currentUserName)
                return Unauthorized();
            
            if (message.SenderUsername == currentUserName) message.SenderDeleted = true;
            if (message.RecipientUsername == currentUserName) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted) 
            {
                uow.MessageRepository.DeleteMessage(message);
            }

            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting the message");
        }
       
    }
}