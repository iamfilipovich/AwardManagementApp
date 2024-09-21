namespace AwardManagementApp.Repositories.Abstract
{
    public interface IUserTimeIntervalsService
    {
        Task CalculateAndSaveAwardsAsync();
        Task SaveUserAwardHistoryAsync(int userId, int awardId, double totalPeriodsSinceRegistration, string periodicType);
    }
}
