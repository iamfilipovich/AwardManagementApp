namespace AwardManagementApp.Repositories.Abstract
{
    public interface IUserTimeIntervalsService
    {
        //RADI
        Task CalculateAndAssignAwardsAsync();
        Task AssignAwardToUser(int userId, int awardId, decimal amount);

    }
}
