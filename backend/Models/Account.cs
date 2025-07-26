namespace Backend.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Iban { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 1000;
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
