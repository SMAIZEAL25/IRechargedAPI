using IRecharge_API.DTO;

namespace IRecharge_API.BLL.AuthService
{
    public interface IAuthManager
    {
        Task<APIResponse<object>> Register(RegisterUserDTO createStudentDTO);
        Task<APIResponse<object>> Login(LoginDto loginDTO);
    }
}
