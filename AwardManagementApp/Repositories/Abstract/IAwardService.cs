
namespace AwardManagementApp.Abstract
{
    public interface IAwardService
    {
        Task CreateAwardAsync(Award award);
        Task<Award> GetAwardByName(string name);
        Task<decimal> GetTotalAwardsAmountByDateAsync(int personalNumber, DateTime date);
        Task BulkInsertUserAwardHistory(IEnumerable<UserAwardHistory> histories);
    }
}
