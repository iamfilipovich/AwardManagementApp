
namespace AwardManagementApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTimeIntervalsController : ControllerBase
    {
        private readonly IUserTimeIntervalsService _userTimeIntervalsService;
        private readonly ILogger<UserTimeIntervalsController> _logger;

        public UserTimeIntervalsController(IUserTimeIntervalsService userTimeIntervalsService, ILogger<UserTimeIntervalsController> logger)
        {
            _userTimeIntervalsService = userTimeIntervalsService;
            _logger = logger;
        }

        [HttpPost("calculate-and-assign")]
        public async Task<IActionResult> CalculateAndAssignAwards()
        {
            try
            {
                await _userTimeIntervalsService.CalculateAndAssignAwardsAsync();
                return Ok("Time intervals calculated and awards assigned successfully.");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
