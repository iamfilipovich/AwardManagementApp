
namespace AwardManagementApp.Repositories.Implement
{
    public class UserService : IUserService
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<UserService> _logger;

        public UserService(IDbConnection dbConnection, ILogger<UserService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<User> RegisterUserAsync(User user)
        {
            try
            {
                var existingUser = await _dbConnection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE PersonalNumber = @PersonalNumber",
                    new { PersonalNumber = user.PersonalNumber });

                if (existingUser != null)
                {
                    throw new Exception("User with this personal number already exists.");
                }

                var result = await _dbConnection.ExecuteAsync(
                    "INSERT INTO Users (PersonalNumber, FirstName, LastName, DateOfRegistration) VALUES (@PersonalNumber, @FirstName, @LastName, @DateOfRegistration)",
                    new
                    {
                        PersonalNumber = user.PersonalNumber,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DateOfRegistration = user.DateOfRegistration
                    });

                if (result == 0)
                {
                    throw new Exception("Failed to register user.");
                }

                _logger.LogInformation("User registered successfully.");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering the user.");
                throw new Exception("An error occurred while registering the user.", ex);
            }
        }

        public async Task<User> GetUserByPersonalNumber(int personalNumber)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE PersonalNumber = @PersonalNumber",
                new { PersonalNumber = personalNumber });
        }

        public async Task<IEnumerable<User>> GetUsersByDayAsync(DateTime date)
        {
            var sql = @"
            SELECT u.Id, u.FirstName, u.LastName, u.PersonalNumber, u.DateOfRegistration
            FROM Users u
            WHERE CAST(u.DateOfRegistration AS DATE) = @Date";

            return await _dbConnection.QueryAsync<User>(sql, new { Date = date });
        }
        public async Task<bool> DeleteUserAsync(int personalNumber)
        {
            try
            {
                var user = await _dbConnection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE PersonalNumber = @PersonalNumber",
                    new { PersonalNumber = personalNumber });

                if (user == null)
                {
                    _logger.LogWarning("User with PersonalNumber {PersonalNumber} not found.", personalNumber);
                    return false; 
                }

                await _dbConnection.ExecuteAsync(
                    "DELETE FROM UserAwardHistory WHERE UserId = @UserId",
                    new { UserId = user.Id });
                
                var result = await _dbConnection.ExecuteAsync(
                    "DELETE FROM Users WHERE PersonalNumber = @PersonalNumber",
                    new { PersonalNumber = personalNumber });

                if (result > 0)
                {
                    _logger.LogInformation("User with PersonalNumber {PersonalNumber} deleted successfully.", personalNumber);
                    return true; 
                }

                _logger.LogWarning("Failed to delete user with PersonalNumber {PersonalNumber}.", personalNumber);
                return false; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting user with PersonalNumber {PersonalNumber}.", personalNumber);
                throw new Exception("An error occurred while deleting the user.", ex);
            }
        }

    }
}
