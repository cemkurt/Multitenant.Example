using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Multitenant.Example.Application.Abstractions;
using Multitenant.Example.Application.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Multitenant.Example.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly IProductService _productService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _userRole;
        private readonly IConfiguration _configuration;
        public UserController(IProductService productService, UserManager<IdentityUser> userManager
            , RoleManager<IdentityRole> userRole, IConfiguration configuration)
        {
            _productService = productService;
            _userManager = userManager;
            _userRole = userRole;
            _configuration = configuration;
        }

        [Route("SecretAdmin"), HttpGet, Authorize(Roles = "AdminSuper")]
        public IActionResult SecretAdmin()
        {
            return Ok("secret admin page");
        }

        [Route("SecretUser"), HttpGet, Authorize(Roles = "User")]
        public IActionResult SecretUser()
        {
            return Ok("secret user page");
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> login(LoginRequest request)
        {
            var model = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (model != null && await _userManager.CheckPasswordAsync(model, request.Password)) 
            {
                var userRoles = await _userManager.GetRolesAsync(model);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });

            }
            else
            {
                return NotFound("not found");
            }
        }



        [HttpGet, Route("users")]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _userManager.Users.ToListAsync());
        }

        [HttpPost, Route("addUser")]
        public async Task<IActionResult> AddUserAsync(UserRequest userRequest)
        {
            var model = new IdentityUser
            {
                Email = userRequest.Email,
                NormalizedEmail = userRequest.Email.ToUpper(),
                EmailConfirmed = true,
                UserName = userRequest.UserName,
                NormalizedUserName = userRequest.UserName.ToUpper(),
                PhoneNumber = userRequest.PhoneNumber,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                AccessFailedCount = 0,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var user = await _userManager.CreateAsync(model, userRequest.Password);

            if (!user.Succeeded)
                return BadRequest(string.Join(",", user.Errors));

            if (user.Succeeded)
            {
                await _userManager.AddToRoleAsync(model, "AdminSuper");

                return Ok(user);
            }
            else
            {
                return BadRequest("false");
            }

        }

        [HttpPost, Route("addRole")]
        public async Task<IActionResult> AddRoleAsync(RoleRequest request)
        {
            var role = await _userRole.CreateAsync(new IdentityRole { Name = request.Name });

            return Ok(role);
        }

        [HttpGet, Route("roles")]
        public async Task<IActionResult> RolesAsync()
        {
            var models = _userRole.Roles.ToList();

            return Ok(models);
        }
    }
}
public class RoleRequest
{
    public string Name { get; set; }
}
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class UserRequest
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public string Password { get; set; }

}