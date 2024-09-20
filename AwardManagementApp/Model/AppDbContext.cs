namespace AwardManagementApp.Model
{
    public class AppDbContext
    {
        public string ConnectionString { get; }

        public AppDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
