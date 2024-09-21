
namespace AwardManagementApp.Repositories.Implement
{
    public class AwardService : IAwardService
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<AwardService> _logger;
        private readonly IMemoryCache _cache;

        public AwardService(IDbConnection dbConnection, ILogger<AwardService> logger, IMemoryCache memoryCache)
        {
            _dbConnection = dbConnection;
            _logger = logger;
            _cache = memoryCache;
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

        public async Task<decimal> GetTotalAwardsAmountByDateAsync(int personalNumber, DateTime endDate)
        {
            try
            {
                var cacheKey = $"UserAwards_{personalNumber}_{endDate:yyyyMMdd}";

                if (_cache.TryGetValue(cacheKey, out decimal totalAmount))
                {
                    _logger.LogInformation("Total awards amount retrieved from cache for personal number: {PersonalNumber} on date: {EndDate}", personalNumber, endDate);
                    return totalAmount;
                }

                var userId = await _dbConnection.QueryFirstOrDefaultAsync<int>(
                    "SELECT Id FROM Users WHERE PersonalNumber = @PersonalNumber", new { PersonalNumber = personalNumber });

                if (userId == 0)
                {
                    _logger.LogWarning("User not found for personal number: {PersonalNumber}", personalNumber);
                    return 0;
                }

                totalAmount = await _dbConnection.QueryFirstOrDefaultAsync<decimal>(
                    "SELECT SUM(Amount) FROM UserAwardHistory WHERE UserId = @UserId AND CAST(AwardedAt AS DATE) = @EndDate",
                    new { UserId = userId, EndDate = endDate.Date }); 

                _cache.Set(cacheKey, totalAmount, TimeSpan.FromHours(1));

                _logger.LogInformation("Total awards amount calculated and cached for personal number: {PersonalNumber} on date: {EndDate}, Amount: {TotalAmount}", personalNumber, endDate, totalAmount);

                return totalAmount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving awards for personal number: {PersonalNumber} on date: {EndDate}", personalNumber, endDate);
                throw;
            }
        }
    
    public async Task BulkInsertUserAwardHistory(IEnumerable<UserAwardHistory> histories)
        {
            using var connection = new SqlConnection(_dbConnection.ConnectionString); 
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, (SqlTransaction)transaction))
                {
                    bulkCopy.DestinationTableName = "UserAwardHistory"; 

                    bulkCopy.ColumnMappings.Add("UserId", "UserId");
                    bulkCopy.ColumnMappings.Add("AwardId", "AwardId");
                    bulkCopy.ColumnMappings.Add("AwardedAt", "AwardedAt");
                    bulkCopy.ColumnMappings.Add("Amount", "Amount");

                    var dataTable = new DataTable();
                    dataTable.Columns.Add("UserId", typeof(int));
                    dataTable.Columns.Add("AwardId", typeof(int));
                    dataTable.Columns.Add("AwardedAt", typeof(DateTime));
                    dataTable.Columns.Add("Amount", typeof(decimal));

                    foreach (var history in histories)
                    {
                        dataTable.Rows.Add(history.UserId, history.AwardId, history.AwardedAt, history.Amount);
                    }

                    await bulkCopy.WriteToServerAsync(dataTable);
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while bulk inserting user award history.");
                throw; 
            }
        }

    }
}
