namespace Syriana_Manager.Components.Model
{
    public class UpdateProfile
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public UpdateTypeEnum UpdateType { get; set; }
    }
    public enum UpdateTypeEnum : byte
    {
        Password,
        Birthday,
        Role
    }
}
