namespace Backend.Models
{
    public class User
    {
        public int Id { get; set; } 
        public string Tc { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; }   
        public string PhoneNumber { get; set; }

        // Her kullanıcının birden fazla hesabı olabilir
        public List<Account> Accounts { get; set; } = new();
    }
}
