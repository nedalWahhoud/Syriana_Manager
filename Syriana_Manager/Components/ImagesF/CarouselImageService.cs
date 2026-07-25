using Microsoft.AspNetCore.Components.Forms;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Components;

namespace Syriana_Manager.Components.ImagesF
{
    public class CarouselImageService(HttpClient http, IWebAssemblyHostEnvironment env, NavigationManager nav)
    {
        private readonly HttpClient _http = http;
        private readonly IWebAssemblyHostEnvironment _env = env;
        private readonly NavigationManager _nav = nav;
        public List<CarouselImage> DownloadedCarouselImage { get; private set; } = [];
        public string GetImageUrl(CarouselImage carouselImage)
        {
            if (carouselImage == null)
                return "/images/sample.jpg";

            if (carouselImage.ImageUrl != null)
            {
                string dbImageUrl = carouselImage.ImageUrl.TrimStart('/');
                // ✅ Füge eine Zufallszahl hinzu, um Cash zu vermeiden.
                string unique = $"?v={carouselImage.LastModified}";
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
                    if (dbImageUrl.StartsWith("CarouselImages/", StringComparison.OrdinalIgnoreCase))
                    {
                        dbImageUrl = dbImageUrl["CarouselImages/".Length..];
                    }
                    string domin = AppConfig.Domin.TrimEnd('/');

                    string completteUrl = $"{domin}/{AppConfig.CarouselImagesproxy}/{dbImageUrl}{unique}";
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
        public async Task<CarouselImage> GetDefaultImage()
        {

            string localImageUrl = _nav.ToAbsoluteUri("Images/DPImage.png").ToString();

            byte[] imageBytes = await _http.GetByteArrayAsync(localImageUrl);
            string base64 = Convert.ToBase64String(imageBytes);

            return new CarouselImage
            {

                ImageBytes = imageBytes,
                ImageUrlLocal = $"data:image/png;base64,{base64}",
            };
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
                // المتصفح سيقوم بتصغير الصورة بطريقة سريعة جداً اعتماداً على GPU/Canvas العميل
                // يمكنك تغيير الأبعاد إلى 2016 و 1512 أو ما يناسبك
                var resizedFile = await imageFile.RequestImageFileAsync("image/jpeg", 2016, 1512);

                // قراءة الصورة المصغرة جاهزة مباشرة
                using var stream = resizedFile.OpenReadStream(maxAllowedSize: 30 * 1024 * 1024);
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
        // async
        public async Task<List<CarouselImage>> GetAllCarouselAsync()
        {
            if (DownloadedCarouselImage.Count > 0)
                return DownloadedCarouselImage;

            try
            {
                var response = await _http.GetAsync("api/Carousel/getAllCarouselImages");
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
                var carouselImages = await response.Content.ReadFromJsonAsync<List<CarouselImage>>();
                if (carouselImages == null)
                {
                    return [];
                }

                // add to local list
                AddProductToLocal(carouselImages);

                return carouselImages;
            }
            catch
            {
                return [];
            }
        }
        public async Task<ValidationResult> AddCarouselImageAsync(CarouselImage carouselImage)
        {
            if (carouselImage == null || carouselImage.ImageBytes == null || carouselImage.ImageBytes.Length == 0)
            {
                return new ValidationResult { Result = false, Message = "Bilddaten sind erforderlich." };
            }
            if (carouselImage == null || carouselImage.ImageBytes == null || carouselImage.ImageBytes.Length == 0)
            {
                return new ValidationResult { Result = false, Message = "" };
            }
            try
            {
                var response = await _http.PostAsJsonAsync("api/Carousel/addCarouselImage", carouselImage);
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();


                if (!response.IsSuccessStatusCode)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }

                if(result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<CarouselImage> GetCarouselImageByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Carousel/getCarouselImageById/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var carouselImage = await response.Content.ReadFromJsonAsync<CarouselImage>();
                    if (carouselImage != null)
                    {
                        return carouselImage;
                    }
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public async  Task<ValidationResult> UpdateCarouselImageAsync(CarouselImage carouselImage)
        {
            if (carouselImage == null)
            {
                return new ValidationResult { Result = false, Message = "Carousel daten sind erforderlich." };
            }
            try
            {
                var response = await _http.PutAsJsonAsync("api/Carousel/updateCarouselImage", carouselImage);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                }
                // update local list
                var index = DownloadedCarouselImage.FindIndex(ci => ci.Id == carouselImage.Id);
                if (index != -1)
                {
                    DownloadedCarouselImage[index] = carouselImage;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteCarouselImageAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Carousel/deleteCarouselImage/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                }
                // remove from local list
                DownloadedCarouselImage.RemoveAll(ci => ci.Id == id);
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // local
        public void AddProductToLocal(CarouselImage carouselImage)
        {
            if (!DownloadedCarouselImage.Any(p => p.Id == carouselImage.Id))
            {
                DownloadedCarouselImage.Add(carouselImage);
            }
        }
        public void AddProductToLocal(List<CarouselImage> carouselImage)
        {
            if (carouselImage.Count > 0 && DownloadedCarouselImage.Count == 0)
            {
                DownloadedCarouselImage.AddRange(carouselImage);
                return;
            }
            foreach (var product in carouselImage)
            {
                if (!DownloadedCarouselImage.Any(p => p.Id == product.Id))
                {
                    DownloadedCarouselImage.Add(product);
                }
            }
        }
    } 
}
