using IRecharge_API.DTO;
using IRechargedAPI.BLL.AuthService;

namespace IRecharge_API.BLL.AuthService
{
    public interface IAuthManager
    {
        Task<APIResponse<AuthReponse>> Register(RegisterUserDTO createStudentDTO);
        Task<APIResponse<object>> Login(LoginDto loginDTO);
    }
}
