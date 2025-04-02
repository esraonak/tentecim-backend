namespace TentecimApi.Models
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
