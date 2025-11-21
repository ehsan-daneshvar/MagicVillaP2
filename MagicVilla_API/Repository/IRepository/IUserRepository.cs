using MagicVilla_API.Models;
using MagicVilla_API.Models.DTO;
using System.Linq.Expressions;

namespace MagicVilla_API.Repository.IRepository
{
    public interface IUserRepository /*: IRepository<Villa>*/
    {
        bool IsUniqueUser(string username);
        Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
        Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO);
        Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO);
        Task RevokeRefreshToken(TokenDTO tokenDTO);
    }
}
