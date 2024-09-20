using System.ComponentModel.DataAnnotations.Schema;

namespace AwardManagementApp.Model
{
    public class Award
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string PeriodicType { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime DateOfAward { get; set; }
    }
}
