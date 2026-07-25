namespace Syriana_Manager.Components.Model
{
    public class ColumnOption(string label, string key, bool isVisible)
    {

        public string Label { get; set; } = label;
        public string Key { get; set; } = key;
        public bool IsVisible { get; set; } = isVisible;
    }
}
