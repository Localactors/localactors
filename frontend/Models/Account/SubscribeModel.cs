using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;


    public class SubscribeModel
    {

        [Required]
        [Email]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        [EqualTo("Password")]
        public string Confirm { get; set; }

        [Required]
        public string Privacy { get; set; }

        [Required]
        public string Terms { get; set; }

        [Required]
        public bool Newsletter { get; set; }

        public string recaptcha_challenge_field { get; set; }
        public string recaptcha_response_field { get; set; }
    }
