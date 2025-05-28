using IRechargedAPI.Infrastruture.BLL.AuthService;
using IRechargedAPI.Presentation.DTO;

namespace IRechargedAPI.Domian.Interface
{
    public interface IAuthManager
    {
        Task<APIResponse<AuthReponse>> Register(RegisterUserDTO createStudentDTO);
        Task<APIResponse<AuthReponse>> Login(LoginDto loginDTO);
    }
}
