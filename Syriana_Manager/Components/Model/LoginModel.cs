using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Syriana_Manager.Components.Model
{
    public class LoginModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "die Email ist erforderlich.")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public bool IsGuest { get; set; }
        public string SignupProvider { get; set; } = string.Empty;
        [JsonIgnore]
        public bool RememberMe { get; set; } = false;
        public string Token { get; set; } = string.Empty;
    }
}
