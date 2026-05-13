using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase {
        private readonly IRoomRepository _roomRepo;
        public RoomController(IRoomRepository roomRepository) {
            _roomRepo = roomRepository;
        }
    }
}
