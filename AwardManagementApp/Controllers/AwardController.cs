
namespace AwardManagementApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwardsController : ControllerBase
    {
        private readonly IAwardService _awardService;
        private readonly IUserTimeIntervalsService _userTimeIntervalsService;
        private readonly ILogger<AwardsController> _logger;

        public AwardsController(IAwardService awardService, IUserTimeIntervalsService userTimeIntervalsService, ILogger<AwardsController> logger)
        {
            _awardService = awardService;
            _userTimeIntervalsService = userTimeIntervalsService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAward(AwardDto awardModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            var existingAward = await _awardService.GetAwardByName(awardModel.Name);
            if (existingAward != null)
            {
                return Conflict("Award with the same name already exists.");
            }

            var award = new Award
            {
                Name = awardModel.Name,
                Amount = awardModel.Amount,
                PeriodicType = awardModel.PeriodicType,
            };

            try
            {
                await _awardService.CreateAwardAsync(award);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Award created successfully.");
        }
        [HttpPost("calculate-awards")]
        public async Task<IActionResult> CalculateAwards()
        {
            try
            {
                await _userTimeIntervalsService.CalculateAndSaveAwardsAsync();
                _logger.LogInformation("Awards successfully calculated and saved.");
                return Ok(new { Message = "Awards calculated and saved successfully for all users." });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calculating awards.");
                return StatusCode(500, "An error occurred while calculating awards.");
            }
        }
        [HttpGet("user-awards")]
        public async Task<IActionResult> GetTotalAwardsByDate([FromQuery] int personalNumber, [FromQuery] DateTime endDate)
        {
            try
            {
                var totalAmount = await _awardService.GetTotalAwardsAmountByDateAsync(personalNumber, endDate);

                if (totalAmount == 0)
                {
                    _logger.LogWarning("No awards found for the user with personal number: {PersonalNumber}", personalNumber);
                    return NotFound(new { Message = "No awards found for the user." });
                }

                return Ok(new { TotalAwardsAmount = totalAmount });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving awards for personal number: {PersonalNumber}", personalNumber);
                return StatusCode(500, "An error occurred while retrieving awards.");
            }
        }
    }
}
