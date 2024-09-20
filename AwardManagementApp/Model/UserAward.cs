using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AwardManagementApp.Model
{
    public class UserAward
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AwardId { get; set; }
        public int Amount { get; set; }
        public DateTime AwardedAt { get; set; }
    }
}
