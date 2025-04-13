namespace IRecharge_API.BLL.AuthService
{
    public class TokenResult
    {
        public string Token { get; set; }
        public DateTime ExpiresIn { get; set; }
        public string UserId { get; set; }
        public string ErrorMessage { get; set; }
    }



}
