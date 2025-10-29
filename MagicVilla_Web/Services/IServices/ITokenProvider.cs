using MagicVilla_Web.Models.DTO;

namespace MagicVilla_Web.Services.IServices
{
    public interface ITokenProvider
    {
        public void SetToken (TokenDTO tokenDTO);
        public void ClearToken();
        public TokenDTO? GetToken ();
    }
}
