namespace AwardManagementApp.Model
{
    public class UserAwardHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AwardId { get; set; }
        public decimal Amount { get; set; }
        public DateTime AwardedAt { get; set; }
    }
}
