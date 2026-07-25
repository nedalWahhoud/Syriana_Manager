using Syriana_Manager.Components.Model;

namespace Syriana_Manager.Components.CategoriesF
{
    public class CategoryService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<ValidationResult> CreateCategory(Categories category)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Categories/createCategory", category);
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht erstellt werden." };
                }
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = result?.Message ?? "Die Kategorie konnte nicht erstellt werden." };
                }
                if (result != null && result.Result)
                {
                    // Add to local list
                    category.Id = result.NewId ?? 0;
                    AddCategoriesToLocal(category);
                }
                else
                {
                    return new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht erstellt werden." };
                }
                return result;
            }
            catch
            {
                return new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht erstellt werden." };
            }
        }

        public List<Categories> DownloadedCategories { get; set; } = [];
        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            // if DownloadedCategories already has items, return them
            if (DownloadedCategories.Count > 0)
            {
                return DownloadedCategories;
            }
            try
            {
                GetItems<Categories> getItems = new () { IsAdmin = true};

                var response = await _http.PostAsJsonAsync($"api/Categories/getCategories",getItems);
                if (!response.IsSuccessStatusCode)
                    return [];

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>() ?? new();
                // add the categories to the local list
                AddCategoriesToLocal(getItems?.Items ?? []);

                return getItems?.Items ?? [];
            }
            catch
            {
                return [];
            }
        }
        public async Task<Categories> GetCategoryById(int categoryId)
        {
            if (categoryId <= 0)
                return null!;

            // check if the category is already downloaded
            var existingCategory = DownloadedCategories.FirstOrDefault(c => c.Id == categoryId);
            if (existingCategory != null)
                return existingCategory;

            try
            {
                var response = await _http.GetAsync($"api/Categories/getCategoryById/{categoryId}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var category = await response.Content.ReadFromJsonAsync<Categories>();
                // add to local list if not exists
                if (category != null)
                {
                    DownloadedCategories.Add(category);
                }
                return category!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<ValidationResult> UpdateCategoryAsync(Categories category)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/Categories/updateCategory", category);
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht aktualisiert werden." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = result?.Message ?? "Die Kategorie konnte nicht aktualisiert werden." };
                }
                // update local list
                int index = DownloadedCategories.FindIndex(c => c.Id == category.Id);
                if (index != -1)
                {
                    DownloadedCategories[index] = category;
                }
                return result;
            }
            catch
            {
                return new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht aktualisiert werden." };
            }
        }
        public async Task<ValidationResult> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Categories/deleteCategory/{id}");
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                    return result ?? new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht gelöscht werden." };

                if (result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht gelöscht werden." };
                }
                // remove from local list
                var localCategory = DownloadedCategories.FirstOrDefault(c => c.Id == id);
                if (localCategory != null)
                {
                    DownloadedCategories.Remove(localCategory);
                }
                return result;
            }
            catch
            {
                return new ValidationResult { Result = false, Message = "Die Kategorie konnte nicht gelöscht werden." };
            }
        }
     
        // Local
        public void AddCategoriesToLocal(List<Categories> categories)
        {
            if (categories.Count > 0 && DownloadedCategories.Count == 0)
            {
                DownloadedCategories.AddRange(categories);
                return;
            }

            foreach (var category in categories)
            {
                if (!DownloadedCategories.Any(p => p.Id == category.Id))
                {
                    DownloadedCategories.Add(category);
                }
            }
        }
        public void AddCategoriesToLocal(Categories category)
        {
            if (!DownloadedCategories.Any(p => p.Id == category.Id))
            {
                DownloadedCategories.Add(category);
            }
        }
        public bool IsCategoryEdited(Categories currentCategory, Categories editCategory)
        {
            if (currentCategory.Name_de != editCategory.Name_de ||
                currentCategory.Name_ar != editCategory.Name_ar ||
                currentCategory.Requires18Plus != editCategory.Requires18Plus ||
                currentCategory.IsAktiv != editCategory.IsAktiv)
            {
                return true;
            }
            return false;
        }
    }
}
