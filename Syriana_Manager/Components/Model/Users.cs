using System.ComponentModel;

namespace Syriana_Manager.Components.Model
{
    public class Users
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        private string _role = string.Empty;
        public string Role
        {
            get => _role;
            set => _role = value?.ToLower() ?? string.Empty; 
        }
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public bool IsGuest { get; set; } 
        public bool IsAktiv { get; set; }
        public string SignupProvider { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } 
    }
    public enum UserRole
    {
        Admin,
        User
    }
}
