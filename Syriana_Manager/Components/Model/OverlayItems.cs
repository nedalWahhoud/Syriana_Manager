namespace Syriana_Manager.Components.Model
{
    public class OverlayItems
    {
        public string Title { get; set; } = string.Empty;
        public List<(string,OverlayType)> Items { get; set; } = [];
        public string NoData { get; set; } = string.Empty;

    }
    public enum OverlayType
    {
        AddSN,
        UpdateSN,
        None
    }
}
