using System.ComponentModel.DataAnnotations;

public class DonationDetails {
    [Required]
    public int InvestmentID { get; set; }
    [Required]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }
    [Required]
    public string Currency { get; set; }
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }
}