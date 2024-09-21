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
                userAwardTotals.Add(new
                {
                    FullName = $"{user.FirstName} {user.LastName}",
                });
            }

            return userAwardTotals;
        }
        
    }
}
