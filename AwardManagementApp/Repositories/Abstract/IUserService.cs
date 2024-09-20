namespace AwardManagementApp.Repositories.Abstract
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(User user);
        Task<User> GetUserByPersonalNumber(int personalNumber);
        Task<IEnumerable<User>> GetUsersByDayAsync(DateTime date);
        Task<bool> DeleteUserAsync(int personalNumber);
    }
}
