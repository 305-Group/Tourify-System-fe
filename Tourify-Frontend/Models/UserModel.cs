namespace Tourify_Frontend.Models
{
    public class UserModel
    {
        public class UserRegister
        {
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Role { get; set; }
            public string Status { get; set; }
        }

        public class VerifyOTPDTO
        {
            public string Email { get; set; }
            public string Otp { get; set; }
        }
    }
}
