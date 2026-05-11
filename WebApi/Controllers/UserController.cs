using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly IUserRepository _userRepo;
        public UserController(IUserRepository userRepository) {
            _userRepo = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user) {
            User res = await _userRepo.CreateAsync(user);

            return Ok(user);
        }
    }
}
