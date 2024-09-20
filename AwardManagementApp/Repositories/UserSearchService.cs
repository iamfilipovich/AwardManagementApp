namespace AwardManagementApp.Repositories
{
    public class UserSearchService
    {
        private readonly IUserService _userService;
        private readonly IAwardService _awardService;

        public UserSearchService(IUserService userService, IAwardService awardService)
        {
            _userService = userService;
            _awardService = awardService;
        }
        public async Task<List<object>> GetUsersByDayAsync(DateTime date)
        {
            var users = await _userService.GetUsersByDayAsync(date);
            var userAwardTotals = new List<object>();

            foreach (var user in users)
            {
                var totalAmount = await _awardService.GetTotalAwardsAmountAsync(user.Id);

                userAwardTotals.Add(new
                {
                    FullName = $"{user.FirstName} {user.LastName}",
                });
            }

            return userAwardTotals;
        }

        public async Task<object> GetUserAwardsByPersonalNumberAndDateAsync(int personalNumber, DateTime date)
        {
            var user = await _userService.GetUserByPersonalNumber(personalNumber);

            if (user == null)
            {
                return null; 
            }

            var totalAmount = await _awardService.GetTotalAwardsAmountByDateAsync(user.Id, date);

            return new
            {
                FullName = $"{user.FirstName} {user.LastName}",
                PersonalNumber = user.PersonalNumber,
                Date = date.ToString("yyyy-MM-dd"),
                TotalAwards = totalAmount
            };
        }
    }
}
