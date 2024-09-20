
namespace AwardManagementApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwardsController : ControllerBase
    {
        private readonly IAwardService _awardService;

        public AwardsController(IAwardService awardService)
        {
            _awardService = awardService;
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
    }
}
