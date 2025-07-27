using Backend.Models;

public class Bill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }

    public  User? User { get; set; }
}
