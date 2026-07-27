namespace Syriana_Manager.Components.Model
{
    public static class AppConfig
    {
        public const string Version = "1.0.0";

#if DEBUG
        public const string ApiBaseUrl = "https://localhost:7250";
        public static Uri ApiUri => new(ApiBaseUrl);
        public const string Domin = "https://localhost:7078";
        public const string ProductImagesproxy = "ProductImages";
        public const string CarouselImagesproxy = "CarouselImages";
        public const string WebRequestProductImagePath = "api/ShareStorage";
        public const string GoogleApiUrl = "https://localhost:7250/api/users/google-login";
#else
        public const string ApiBaseUrl =  "https://syriana-supermarkt.de/"; /* in jede Request schreiben wir api und so wird http://217.154.152.128:1081/api und in Server eine Proxy to localhost:2030*/
        public static Uri ApiUri => new Uri(ApiBaseUrl);
        public const string Domin  = "https://syriana-supermarkt.de";
        public const string ProductImagesproxy  = "ProductImages";
        public const string CarouselImagesproxy  = "CarouselImages";
        public const string WebRequestProductImagePath  = "api/ShareStorage";
        public const string GoogleApiUrl = "/users/google-login";
#endif



    }
}
