namespace AwardManagementApp.Model.DTO
{
    public class TimeSpanIntervals
    {
        public int Days { get; set; }
        public int Weeks { get; set; }
        public int Months { get; set; }
        public double Hours { get; set; }
        public List<Award> Awards { get; set; } 
    }
}
