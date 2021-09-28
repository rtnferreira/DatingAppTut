using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        /* private readonly DataContext _context; */
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;

        private readonly IMapper _mapper;

        public AccountController(/* DataContext context */ UserManager<AppUser> userManager, SignInManager<AppUser> signInManager
                                , ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            /* _context = context; */
            _tokenService = tokenService;
            _mapper = mapper;

        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
        {

            if (await UserExists(registerDTO.Username))
                return BadRequest("Username is taken");


            /* using var hmac = new HMACSHA512(); */

            /*             var user = new AppUser
                        {
                            UserName = registerDTO.Username.ToLower(),
                            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                            PasswordSalt = hmac.Key
                        }; */

            var user = _mapper.Map<AppUser>(registerDTO);

            user.UserName = registerDTO.Username.ToLower();
            /* user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key; */

            /* _context.Users.Add(user);
            await _context.SaveChangesAsync(); */

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if(!result.Succeeded)
                return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            if(!roleResult.Succeeded)
                return BadRequest(result.Errors);

            /* return user; */
            return new UserDTO
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
                ,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {

            var user = await _userManager.Users
                             .Include(p => p.Photos)
                             .SingleOrDefaultAsync(s => s.UserName == loginDTO.Username.ToLower());

            if (user == null)
                return Unauthorized("Invalid username");

            /* using var hmac = new HMACSHA512(user.PasswordSalt);
            
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

            for(int i=0; i < computedHash.Length; i++)
            {
                if(computedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid password");
            }
            */

            var result = await _signInManager
                            .CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if(!result.Succeeded)
                return Unauthorized();

            /* return user; */
            return new UserDTO
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(s => s.IsMain)?.Url
                ,
                KnownAs = user.KnownAs
                ,
                Gender = user.Gender
            };
        }

        
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(s => s.UserName == username.ToLower());
        }
    }
}