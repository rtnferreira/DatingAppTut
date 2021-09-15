using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        /* private readonly DataContext _context; */
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task< ActionResult<IEnumerable<MemberDTO>> > GetUsers()
        {
            /* return await _context.Users.ToListAsync(); */

            /* var users = await _userRepository.GetUsersAsync();
            var usersToReturn =  _mapper.Map<IEnumerable<MemberDTO>>(users); */

            var users = await _userRepository.GetMembersAsync();
            return Ok(users);
        }

        // api/users/3
        /* [HttpGet("{id}")] */
        [HttpGet("{username}")]
        [Authorize]
        public async Task< ActionResult<MemberDTO> > GetUser(string username)
        {
            /* return  await _context.Users.FindAsync(id); */

            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByUserNameAsync(username);
            
            _mapper.Map(memberUpdateDTO, user);

            _userRepository.Update(user);

            if(await _userRepository.SaveAllAsync())
                return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}