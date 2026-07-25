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
        public const string ApiBaseUrl =  "http://localhost:2030";
        public static Uri ApiUri => new Uri(ApiBaseUrl);
        public const string Domin  = "https://syriana-supermarkt.de";
        public const string ProductImagesproxy  = "ProductImages";
        public const string CarouselImagesproxy  = "CarouselImages";
        public const string WebRequestProductImagePath  = "api/ShareStorage";
        public const string GoogleApiUrl = "/users/google-login";
#endif



    }
}
