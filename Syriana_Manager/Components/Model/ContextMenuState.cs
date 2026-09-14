namespace Syriana_Manager.Components.Model
{
    public class ContextMenuState
    {
        public bool Show { get; set; } = false;
        public double X { get; set; }
        public double Y { get; set; }
        public void ShowMenu(double x, double y)
        {
            X = x;
            Y = y;
            Show = true;
        }

        public void HideMenu()
        {
            Show = false;
        }
    }
}
