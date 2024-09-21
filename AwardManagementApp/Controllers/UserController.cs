
using AwardManagementApp.Model;

namespace AwardManagementApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly UserSearchService _userSearchService;
        private readonly IAwardService _awardService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, UserSearchService userSearchService, 
                                IAwardService awardService, ILogger<UserController> logger)
        {
            _userService = userService;
            _userSearchService = userSearchService;
            _awardService = awardService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var existingUser = await _userService.GetUserByPersonalNumber(userModel.PersonalNumber);
            if (existingUser != null)
            {
                return Conflict("User with the same personal number already exists.");
            }

            var user = new User
            {
                FirstName = userModel.FirstName,
                LastName = userModel.LastName,
                PersonalNumber = userModel.PersonalNumber,
                DateOfRegistration = userModel.DateOfRegistration
            };

            await _userService.RegisterUserAsync(user);

            return Ok("User registered successfully.");
        }

        [HttpGet("search-by-day")]
        public async Task<IActionResult> SearchByDay([FromQuery] DateTime date)
        {
            try
            {
                var result = await _userSearchService.GetUsersByDayAsync(date);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{personalNumber}")]
        public async Task<IActionResult> DeleteUser(int personalNumber)
        {
            try
            {
                var deleted = await _userService.DeleteUserAsync(personalNumber);

                if (deleted)
                {
                    _logger.LogInformation("User with PersonalNumber {PersonalNumber} deleted.", personalNumber);
                    return NoContent(); 
                }

                _logger.LogWarning("User with PersonalNumber {PersonalNumber} not found.", personalNumber);
                return NotFound(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user with PersonalNumber {PersonalNumber}.", personalNumber);
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }
    }

}
