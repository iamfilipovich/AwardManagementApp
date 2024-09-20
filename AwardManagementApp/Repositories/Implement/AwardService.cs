using Dapper;
using System.Data;
using System.Threading.Tasks;
using AwardManagementApp.Model;
using Microsoft.Extensions.Logging;

namespace AwardManagementApp.Repositories.Implement
{
    public class AwardService : IAwardService
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<AwardService> _logger;

        public AwardService(IDbConnection dbConnection, ILogger<AwardService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task CreateAwardAsync(Award award)
        {
            var awardExists = await _dbConnection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(1) FROM Awards WHERE Name = @Name",
                new { Name = award.Name });

            if (awardExists > 0)
            {
                throw new ArgumentException("Award with the same name already exists.");
            }

            var result = await _dbConnection.ExecuteAsync(
                "INSERT INTO Awards (Name, Amount, PeriodicType, UserId) VALUES (@Name, @Amount, @PeriodicType, @UserId)",
                new
                {
                    Name = award.Name,
                    Amount = award.Amount,
                    PeriodicType = award.PeriodicType,
                    UserId = award.UserId
                });

            if (result == 0)
            {
                throw new Exception("Failed to create award.");
            }
        }

        public async Task<bool> AwardExistsAsync(int userId, string periodicType)
        {
            var query = "SELECT COUNT(1) FROM Awards WHERE UserId = @UserId AND PeriodicType = @PeriodicType";
            var exists = await _dbConnection.ExecuteScalarAsync<int>(query, new { UserId = userId, PeriodicType = periodicType });
            return exists > 0;
        }

        public async Task<Award> GetAwardByName(string name)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<Award>(
                "SELECT * FROM Awards WHERE Name = @Name",
                new { Name = name });
        }

        public async Task<int> GetTotalAwardsAmountAsync(int userId)
        {
            var sql = @"
            SELECT SUM(a.Amount) 
            FROM Awards a
            WHERE a.UserId = @UserId";

            var result = await _dbConnection.ExecuteScalarAsync<int>(sql, new { UserId = userId });
            return result;
        }

        public async Task<int> GetTotalAwardsAmountByDateAsync(int userId, DateTime endDate)
        {
            //    string query = @"
            //        SELECT ISNULL(SUM(A.Amount), 0) 
            //        FROM Awards A
            //        JOIN Users U ON A.UserId = U.Id
            //        WHERE A.UserId = @UserId 
            //        AND DATEADD(DAY, 
            //        CASE 
            //            WHEN A.PeriodicType = 'Hourly' THEN 1
            //            WHEN A.PeriodicType = 'Daily' THEN 5
            //            WHEN A.PeriodicType = 'Weekly' THEN 10
            //            WHEN A.PeriodicType = 'Monthly' THEN 100
            //            ELSE 0
            //        END, 
            //    U.DateOfRegistration) <= @EndDate";

            //    _logger.LogInformation("Executing query: {Query} with parameters: UserId = {UserId}, EndDate = {EndDate}", query, userId, endDate);

            //    var result = await _dbConnection.ExecuteScalarAsync<int>(query, new { UserId = userId, EndDate = endDate });

            //    _logger.LogInformation("Result: {Result}", result);

            //    return result;
            string query = @"
        SELECT 
            ISNULL(SUM(
                CASE 
                    WHEN A.PeriodicType = 'Hourly' THEN 
                        -- Calculate total hours between registration and end date
                        (DATEDIFF(HOUR, U.DateOfRegistration, @EndDate) + 1) * A.Amount
                    WHEN A.PeriodicType = 'Daily' THEN 
                        -- Calculate total days between registration and end date
                        (DATEDIFF(DAY, U.DateOfRegistration, @EndDate) + 1) * A.Amount
                    WHEN A.PeriodicType = 'Weekly' THEN 
                        -- Calculate total weeks between registration and end date
                        (DATEDIFF(WEEK, U.DateOfRegistration, @EndDate) + 1) * A.Amount
                    WHEN A.PeriodicType = 'Monthly' THEN 
                        -- Calculate total months between registration and end date
                        (DATEDIFF(MONTH, U.DateOfRegistration, @EndDate) + 1) * A.Amount
                    ELSE 0
                END
            ), 0) AS TotalAwardsAmount
        FROM Awards A
        JOIN UserAward UA ON A.Id = UA.AwardId
        JOIN Users U ON UA.UserId = U.Id
        WHERE U.Id = @UserId
            AND U.DateOfRegistration <= @EndDate";

            _logger.LogInformation("Executing query: {Query} with parameters: UserId = {UserId}, EndDate = {EndDate}", query, userId, endDate);

            var result = await _dbConnection.ExecuteScalarAsync<int>(query, new { UserId = userId, EndDate = endDate });

            _logger.LogInformation("Result: {Result}", result);

            return result;
        }

    }
}
