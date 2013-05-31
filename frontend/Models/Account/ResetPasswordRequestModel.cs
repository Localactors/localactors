using DataAnnotationsExtensions;

    public class ResetPasswordRequestModel
    {
        [Email]
        public string Email { get; set; }
        public string Username { get; set; }
    }
