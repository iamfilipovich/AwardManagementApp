namespace AwardManagementApp.Model.DTO
{
    public class UserRegistrationDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int PersonalNumber { get; set; }

        //vrijeme kod dodavanja usera pisati u ovom obliku "yyyy-mm-ddThh:mm:ss"
        public DateTime DateOfRegistration { get; set; }
    }
}
