
namespace AwardManagementApp.Abstract
{
    public interface IAwardService
    {
        Task CreateAwardAsync(Award award);
        Task<Award> GetAwardByName(string name);
        Task<int> GetTotalAwardsAmountAsync(int userId);
        Task<decimal> GetTotalAwardsAmountByDateAsync(int personalNumber, DateTime endDate);
    }
}
