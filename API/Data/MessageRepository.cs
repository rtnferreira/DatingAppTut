using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
             _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(c => c.Connections)
                                 .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                                 .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                                 .Include(x => x.Sender)
                                 .Include(x => x.Recipient)
                                 .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections).FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent)
                                         .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
                                         .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username 
                                            && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                                             && u.SenderDeleted == false),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username 
                                      && u.DateRead == null && u.RecipientDeleted == false)
            };

            /* var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider); */

            return await PagedList<MessageDTO>.CreateAsync(/* messages */ query, messageParams.PageNumber, messageParams.PageSize);
        }

         public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                                 /* .Include(u => u.Sender).ThenInclude(p => p.Photos)
                                 .Include(u => u.Recipient).ThenInclude(p => p.Photos) */
                                 .Where(m => m.Recipient.UserName == currentUserName && m.RecipientDeleted == false
                                             && m.Sender.UserName == recipientUserName
                                         || m.Recipient.UserName == recipientUserName
                                             && m.Sender.UserName == currentUserName && m.SenderDeleted == false
                                 )
                                 .OrderBy(m => m.MessageSent)
                                 .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
                                 .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null 
                                                     && m.RecipientUsername == currentUserName).ToList();

            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }

            /* return _mapper.Map<IEnumerable<MessageDTO>>(messages); */
            return messages;
        } 

/*         public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername,
            string recipientUsername)
        {
            var messages = await _context.Messages
                .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
                        && m.Sender.UserName == recipientUsername
                        || m.Recipient.UserName == recipientUsername
                        && m.Sender.UserName == currentUsername && m.SenderDeleted == false
                )
                .MarkUnreadAsRead(currentUsername)
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();
 
            return messages;
        } */

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        /* public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        } */
    }
}