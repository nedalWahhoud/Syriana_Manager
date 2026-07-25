using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Components;

namespace Syriana_Manager.Components.ImagesF
{
    public class ProductImagesService(HttpClient http, IWebAssemblyHostEnvironment env, NavigationManager nav)
    {
        private readonly IWebAssemblyHostEnvironment _env = env;
        private readonly HttpClient _http = http;
        private readonly NavigationManager _nav = nav;
        public string GetProductImageUrl(ProductImages productImages)
        {
            if (productImages == null)
                return "/images/sample.jpg";

            if (productImages.ImageUrl != null)
            {
                string dbImageUrl = productImages.ImageUrl.TrimStart('/');
                // ✅ Füge eine Zufallszahl hinzu, um Cash zu vermeiden.
                string unique = $"?v={productImages.LastModified}";
                //
                if (_env.IsDevelopment())
                {
                    string baseUri = AppConfig.ApiUri.ToString().TrimEnd('/');
                    string path = AppConfig.WebRequestProductImagePath.Trim('/');

                    string completteUrl = $"{baseUri}/{path}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
                else
                {
                    if (dbImageUrl.StartsWith("ProductsImages/", StringComparison.OrdinalIgnoreCase))
                    {
                        dbImageUrl = dbImageUrl["ProductsImages/".Length..];
                    }
                    string domin = AppConfig.Domin.TrimEnd('/');

                    string completteUrl = $"{domin}/{AppConfig.ProductImagesproxy}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "DPImage.png");
                var relativePath = path.Split("wwwroot")[1].Replace("\\", "/");

                return relativePath;
            }
        }
        public async Task<byte[]> GetImageBytesAndCheckSize(IBrowserFile imageFile)
        {
           
                using var stream = new MemoryStream();
                byte[] imageBytes = null!;

                // Check if the file size exceeds the limit
                if (imageFile.Size > 512000)
                {
                    imageBytes = await CompressImage(imageFile);
                }
                else
                {
                    await imageFile.OpenReadStream().CopyToAsync(stream);
                    imageBytes = stream.ToArray();
                }

                stream.Dispose();

                return imageBytes;
            
        }
        public async Task<byte[]> CompressImage(IBrowserFile imageFile)
        {
            try
            {
                var resizedFile = await imageFile.RequestImageFileAsync("image/jpeg", 800, 800);

                using var stream = resizedFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                using var memoryStream = new MemoryStream();

                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
                return null!;
            }
        }
        public async Task<ProductImages> GetDefaultImage()
        {
            try
            {
                string localImageUrl = _nav.ToAbsoluteUri("Images/DPImage.png").ToString();

                byte[] imageBytes = await _http.GetByteArrayAsync(localImageUrl);
                string base64 = Convert.ToBase64String(imageBytes);

                return new ProductImages
                {
                    ImageBytes = imageBytes,
                    ImageUrlLocal = $"data:image/png;base64,{base64}",
                    IsMain = true
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error loading default image: {ex.Message}");
                return new ProductImages
                {
                    ImageBytes = null,
                    ImageUrlLocal = string.Empty,
                    IsMain = true
                };

            }
        }
    }
}
