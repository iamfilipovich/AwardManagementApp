
namespace AwardManagementApp.Repositories.Implement
{
    public class UserTimeIntervalsService : IUserTimeIntervalsService
    {
        private readonly IDbConnection _dbConnection;
        private readonly IAwardService _awardService;
        private readonly ILogger<UserTimeIntervalsService> _logger;
        private readonly IMemoryCache _cache;

        public UserTimeIntervalsService(IDbConnection dbConnection, IAwardService awardService,
                                        ILogger<UserTimeIntervalsService> logger, IMemoryCache memoryCache)
        {
            _dbConnection = dbConnection;
            _awardService = awardService;
            _logger = logger;
            _cache = memoryCache;
        }

        public async Task CalculateAndSaveAwardsAsync()
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
                    await SaveUserAwardHistoryAsync(user.Id, hourlyAward.Id, hoursSinceRegistration, "Hourly");
                }

                if (dailyAward != null)
                {
                    await SaveUserAwardHistoryAsync(user.Id, dailyAward.Id, daysSinceRegistration, "Daily");
                }

                if (weeklyAward != null)
                {
                    await SaveUserAwardHistoryAsync(user.Id, weeklyAward.Id, weeksSinceRegistration, "Weekly");
                }

                if (monthlyAward != null)
                {
                    await SaveUserAwardHistoryAsync(user.Id, monthlyAward.Id, monthsSinceRegistration, "Monthly");
                }
            }
        }

        public async Task SaveUserAwardHistoryAsync(int userId, int awardId, double totalPeriodsSinceRegistration, string periodicType)
        {
            var user = await _dbConnection.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @UserId", new { UserId = userId });

            if (user == null)
            {
                _logger.LogWarning("User not found for UserId: {UserId}", userId);
                return;
            }

            var awardAmount = await _dbConnection.QueryFirstOrDefaultAsync<decimal>(
                "SELECT Amount FROM Awards WHERE Id = @AwardId", new { AwardId = awardId });

            if (awardAmount == 0)
            {
                _logger.LogWarning("No award amount found for AwardId: {AwardId}", awardId);
                return;
            }

            var awardHistories = new List<UserAwardHistory>();

            for (int i = 1; i <= totalPeriodsSinceRegistration; i++)
            {
                DateTime awardedAt;

                switch (periodicType)
                {
                    case "Hourly":
                        awardedAt = user.DateOfRegistration.AddHours(i);
                        break;
                    case "Daily":
                        awardedAt = user.DateOfRegistration.AddDays(i);
                        break;
                    case "Weekly":
                        awardedAt = user.DateOfRegistration.AddDays(i * 7);
                        break;
                    case "Monthly":
                        awardedAt = user.DateOfRegistration.AddMonths(i);
                        break;
                    default:
                        _logger.LogWarning("Invalid periodic type: {PeriodicType}", periodicType);
                        return; 
                }
                var existingAward = await _dbConnection.QueryFirstOrDefaultAsync<UserAwardHistory>(
                    "SELECT * FROM UserAwardHistory WHERE UserId = @UserId AND AwardId = @AwardId AND AwardedAt = @AwardedAt",
                
                new { UserId = userId, AwardId = awardId, AwardedAt = awardedAt });

                if (existingAward != null)
                {
                    _logger.LogInformation("Award already exists for UserId: {UserId}, AwardId: {AwardId}, AwardedAt: {AwardedAt}", userId, awardId, awardedAt);
                    continue; 
                }

                awardHistories.Add(new UserAwardHistory
                {
                    UserId = userId,
                    AwardId = awardId,
                    AwardedAt = awardedAt,
                    Amount = awardAmount
                });

                _logger.LogInformation("Prepared to save award for UserId: {UserId}, AwardId: {AwardId}, Amount: {Amount}, AwardedAt: {AwardedAt}", userId, awardId, awardAmount, awardedAt);
            }

            if (awardHistories.Any())
            {
                await _awardService.BulkInsertUserAwardHistory(awardHistories);
            }
        }
    }
}
