using Backend.Models;

public class Loan
{
    public int Id { get; set; }
    public int UserId { get; set; } 
    public decimal Amount { get; set; } 
    public int Term { get; set; } // ➕ Yeni alan
    public DateTime ApplicationDate { get; set; } = DateTime.Now;
    public bool IsApproved { get; set; } 
    public bool IsActive { get; set; } = true; 

    public User? User { get; set; }
}
