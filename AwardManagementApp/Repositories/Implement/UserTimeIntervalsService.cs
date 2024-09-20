
namespace AwardManagementApp.Repositories.Implement
{
    public class UserTimeIntervalsService : IUserTimeIntervalsService
    {
        private readonly IDbConnection _dbConnection;
        private readonly IAwardService _awardService;
        private readonly ILogger<UserTimeIntervalsService> _logger;

        public UserTimeIntervalsService(IDbConnection dbConnection, IAwardService awardService, ILogger<UserTimeIntervalsService> logger)
        {
            _dbConnection = dbConnection;
            _awardService = awardService;
            _logger = logger;
        }

        public async Task CalculateAndAssignAwardsAsync()
        {
            var users = await _dbConnection.QueryAsync<User>("SELECT * FROM Users");
            var awards = await _dbConnection.QueryAsync<Award>("SELECT * FROM Awards");

            foreach (var user in users)
            {
                var hoursSinceRegistration = (DateTime.UtcNow - user.DateOfRegistration).TotalHours;
                var daysSinceRegistration = (DateTime.UtcNow - user.DateOfRegistration).Days;
                var weeksSinceRegistration = (DateTime.UtcNow - user.DateOfRegistration).Days / 7;
                var monthsSinceRegistration = (DateTime.UtcNow.Year - user.DateOfRegistration.Year) * 12 + DateTime.UtcNow.Month - user.DateOfRegistration.Month;

                var hourlyAward = awards.FirstOrDefault(a => a.PeriodicType == "Hourly");
                var dailyAward = awards.FirstOrDefault(a => a.PeriodicType == "Daily");
                var weeklyAward = awards.FirstOrDefault(a => a.PeriodicType == "Weekly");
                var monthlyAward = awards.FirstOrDefault(a => a.PeriodicType == "Monthly");

                if (hourlyAward != null)
                {
                    await AssignAwardToUser(user.Id, hourlyAward.Id, (int)hoursSinceRegistration * hourlyAward.Amount);
                }

                if (dailyAward != null)
                {
                    await AssignAwardToUser(user.Id, dailyAward.Id, daysSinceRegistration * dailyAward.Amount);
                }

                if (weeklyAward != null)
                {
                    await AssignAwardToUser(user.Id, weeklyAward.Id, weeksSinceRegistration * weeklyAward.Amount);
                }

                if (monthlyAward != null)
                {
                    await AssignAwardToUser(user.Id, monthlyAward.Id, monthsSinceRegistration * monthlyAward.Amount);
                }
            }
        }

        public async Task AssignAwardToUser(int userId, int awardId, decimal amount)
        {
            var userAwardExists = await _dbConnection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM UserAward WHERE UserId = @UserId AND AwardId = @AwardId",
                new { UserId = userId, AwardId = awardId });

            if (userAwardExists > 0)
            {
                _logger.LogWarning("Award already assigned to user: UserId = {UserId}, AwardId = {AwardId}", userId, awardId);
                return;
            }

            await _dbConnection.ExecuteAsync(
                "INSERT INTO UserAward (UserId, AwardId, Amount, AwardedAt) VALUES (@UserId, @AwardId, @Amount, @AwardedAt)",
                new
                {
                    UserId = userId,
                    AwardId = awardId,
                    Amount = amount,
                    AwardedAt = DateTime.UtcNow
                });

            _logger.LogInformation("Award assigned: UserId = {UserId}, AwardId = {AwardId}, Amount = {Amount}", userId, awardId, amount);
        }

    }
}
