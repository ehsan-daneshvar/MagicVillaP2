using System.ComponentModel.DataAnnotations;

namespace MagicVilla_API.Models.DTO
{
    public class LoginRequestDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
