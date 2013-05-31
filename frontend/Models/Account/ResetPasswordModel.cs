using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;



    public class ResetPasswordModel
    {
        [Required]
        public string Password { get; set; }
        [Required]
        [EqualTo("Password")]
        public string Confirm { get; set; }
        [Required]
        public string Key { get; set; }
    }