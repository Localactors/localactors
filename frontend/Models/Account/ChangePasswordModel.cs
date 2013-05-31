using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

public class ChangePasswordModel
{
    [Required]
    public string newpassword { get; set; }

    [Required]
    [EqualTo("newpassword")]
    public string confirm { get; set; }

    [Required]
    public string oldpassword { get; set; }
}