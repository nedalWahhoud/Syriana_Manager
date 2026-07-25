using Syriana_Manager.Components.CategoriesF;
using Syriana_Manager.Components.Model;
namespace Syriana_Manager.Components.DiscountF
{
    public class DiscountService(HttpClient http,CategoryService categoryService)
    {
        private readonly HttpClient _http = http;
        private readonly CategoryService _categoryService = categoryService;
        public List<DiscountCodes> DownloadedDiscountCodes { get; private set; } = [];
        public List<DiscountCategory> DownloadedDiscountCategory { get; private set; } = [];
        public async Task<ValidationResult> AddDiscountCode(DiscountCodes newDiscountCode)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Discounts/addDiscountCode", newDiscountCode);
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                    return result ?? new ValidationResult { Result = false, Message = "Failed to add discount code." };

                if (result?.Result == true)
                {
                    newDiscountCode.Id = result.NewId!.Value;
                    // Lokale Liste aktualisieren
                    DownloadedDiscountCodes.Add(newDiscountCode);
                }

                return result!;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> AddDiscountCategory(DiscountCategory newDiscountCategory)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Discounts/addDiscountCategory", newDiscountCategory);
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Failed to add discount category." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error occurred." };
                if (result.Result == true)
                {
                    newDiscountCategory.Id = result.NewId!.Value;
                    // Lokale Liste aktualisieren
                    newDiscountCategory.Category = _categoryService.DownloadedCategories.FirstOrDefault(c => c.Id == newDiscountCategory.CategoriesId) 
                        ?? await _categoryService.GetCategoryById(newDiscountCategory.CategoriesId);
                    DownloadedDiscountCategory.Add(newDiscountCategory);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteDiscountCode(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Discounts/deleteDiscountCode/{id}");
                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Löschen." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                // Lokale Liste aktualisieren
                if (result.Result)
                {
                    DownloadedDiscountCodes.RemoveAll(dc => dc.Id == id);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteDiscountCategory(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Discounts/deleteDiscountCategory/{id}");
                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Löschen." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                // Lokale Liste aktualisieren
                if (result.Result)
                {
                    DownloadedDiscountCategory.RemoveAll(dc => dc.Id == id);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateDiscountCode(DiscountCodes updatedDiscountCode)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/Discounts/updateDiscountCode", updatedDiscountCode);
                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Aktualisieren." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                if (result.Result)
                {
                    var index = DownloadedDiscountCodes.FindIndex(dc => dc.Id == updatedDiscountCode.Id);
                    if (index != -1)
                    {
                        DownloadedDiscountCodes[index] = updatedDiscountCode;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateDiscountCategory(DiscountCategory updatedDiscountCategory)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/Discounts/updateDiscountCategory", updatedDiscountCategory);
                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Aktualisieren." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                if (result.Result)
                {
                    var index = DownloadedDiscountCategory.FindIndex(dc => dc.Id == updatedDiscountCategory.Id);
                    if (index != -1)
                    {
                        // get category nach dem Update, Wir nehmen es nicht vom ursprünglichen Variable entgegen, um Konflikte zu vermeiden.
                        updatedDiscountCategory.Category = _categoryService.DownloadedCategories.FirstOrDefault(c => c.Id == updatedDiscountCategory.CategoriesId)
                            ?? await _categoryService.GetCategoryById(updatedDiscountCategory.CategoriesId);
                        DownloadedDiscountCategory[index] = updatedDiscountCategory;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetAllDiscountCodes()
        {
            try
            {
                var response = await _http.GetAsync("api/Discounts/getAllDiscountCodes");
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Abrufen." };
                var discountCodes = await response.Content.ReadFromJsonAsync<List<DiscountCodes>>();
                if (discountCodes != null)
                {
                    DownloadedDiscountCodes = discountCodes;
                    return new ValidationResult { Result = true, Message = "Erfolgreich abgerufen." };
                }
                return new ValidationResult { Result = false, Message = "Keine Discount-Codes gefunden." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetAllDiscountCategory()
        {
            try
            {
                var response = await _http.GetAsync("api/Discounts/getAllDiscountCategories");
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Abrufen." };
                var discountCategory = await response.Content.ReadFromJsonAsync<List<DiscountCategory>>();
                if (discountCategory != null)
                {
                    DownloadedDiscountCategory = discountCategory;
                    return new ValidationResult { Result = true, Message = "Erfolgreich abgerufen." };
                }
                return new ValidationResult { Result = false, Message = "Keine Discount-Kategorien gefunden." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<(ValidationResult validationResult, DiscountCategory discountCategory)> CheckDiscountCategory(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                return (new ValidationResult { Result = false, Message = "Der Code muss genau 8 Zeichen lang sein." }, null!);
            }
            try
            {
                var response = await _http.GetAsync($"api/Discounts/checkDiscountCategory/{code}/{-1}");
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return (result1 ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." }, null!);
                }
                DiscountCategory discountCategory = await response.Content.ReadFromJsonAsync<DiscountCategory>() ?? null!;

                return (new ValidationResult { Result = true, Message = "Code erfolgreich überprüft." }, discountCategory);
            }
            catch (Exception ex)
            {
                return (new ValidationResult { Result = false, Message = $"Fehler: {ex.Message}" }, null!);
            }
        }
        public async Task<(ValidationResult validationResult, DiscountCodes discountCodes)> CheckDiscountCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                return (new ValidationResult { Result = false, Message = "Der Code muss genau 8 Zeichen lang sein." }, null!);
            }
            try
            {
                var response = await _http.GetAsync($"api/Discounts/checkDiscountCode/{code}/{-1}");
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return (result1 ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." }, null!);
                }

                DiscountCodes discountCode = await response.Content.ReadFromJsonAsync<DiscountCodes>() ?? null!;

                return (new ValidationResult { Result = true, Message = "Code erfolgreich überprüft." }, discountCode);

            }
            catch (Exception ex)
            {
                return (new ValidationResult { Result = false, Message = $"Fehler: {ex.Message}" }, null!);
            }
        }
        public bool IsEditedDiscountCode(DiscountCodes original, DiscountCodes edited)
        {
            return original.Code != edited.Code ||
                   original.DiscountAmount != edited.DiscountAmount ||
                   original.UsageLimit != edited.UsageLimit ||
                   original.StartDate != edited.StartDate ||
                   original.EndDate != edited.EndDate ||
                   original.IsActive != edited.IsActive || 
                   original.DiscountType != edited.DiscountType;
        }
        public bool IsEditedDiscountCategory(DiscountCategory original, DiscountCategory edited)
        {
            return original.Code != edited.Code ||
                   original.DiscountAmount != edited.DiscountAmount ||
                   original.UsageLimit != edited.UsageLimit ||
                   original.StartDate != edited.StartDate ||
                   original.EndDate != edited.EndDate ||
                   original.IsActive != edited.IsActive ||
                   original.CategoriesId != edited.CategoriesId ||
                   original.DiscountType != edited.DiscountType;
        }
    }
}
