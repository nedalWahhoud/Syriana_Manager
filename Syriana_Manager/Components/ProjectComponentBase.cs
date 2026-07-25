using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace Syriana_Manager.Components
{
    public class ProjectComponentBase : ComponentBase
    {
        protected CultureInfo GermanCulture = new("de-DE");
        protected bool IsArabic => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";
        protected (bool Initialized, bool ParametersSet, bool AfterRender) IsRendered;

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
    }
}
