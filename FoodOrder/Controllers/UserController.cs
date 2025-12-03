using FoodOrder.Repositories.Common;
using Microsoft.AspNetCore.Mvc;

namespace FoodOrder.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController: ControllerBase
    {
        private readonly ApplicationRepository<UserController> _repo;
        public UserController(ApplicationRepository<UserController> repo)
        {
            _repo = repo;
        }
        [HttpGet]
      
    }
}
