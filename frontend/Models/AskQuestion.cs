using System.ComponentModel.DataAnnotations;

public class AskQuestion
{
    
    [DataType(DataType.Text)]
    public string UserName { get; set; }
    [DataType(DataType.Text)]
    public string ProjectName { get; set; }
    public int ProjectID { get; set; }


    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.MultilineText)]
    public string Question { get; set; }
}