using Syriana_Manager.Components.Model;
namespace Syriana_Manager.Components.ProductGroupF
{
    public class ProductGroupService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<GroupProducts> DownloadedproductGroups { get; private set; } = [];
        public async Task<List<GroupProducts>> GetAllGroupProductsAsync()
        {
            if (DownloadedproductGroups.Count != 0)
                return DownloadedproductGroups;

            try
            {
                var response = await _http.GetAsync("api/GroupProducts/getAllGroupProducts");

                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }

                var groupProducts = await response.Content.ReadFromJsonAsync<List<GroupProducts>>() ?? [];

                // add to local list
                DownloadedproductGroups.AddRange(groupProducts);

                return DownloadedproductGroups;
            }
            catch
            {
                return [];
            }
        }
        public async Task <ValidationResult> CreateGroupProduct(GroupProducts groupProduct)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/GroupProducts/CreateGroupProduct", groupProduct);

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Die Produktgruppe konnte nicht erstellt werden.." };
                }
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = result?.Message ?? "Die Produktgruppe konnte nicht erstellt werden." };
                }
                
                if (result != null && result.Result)
                {
                    // Add to local list
                    groupProduct.Id = result.NewId ?? 0;
                    AddCategoriesToLocal(groupProduct);
                }
                else
                {
                    return new ValidationResult { Result = false, Message = result?.Message ?? "keine Id in Message gefunden." };
                }
                
                return new ValidationResult { Result = true, Message = "Produktgruppe erfolgreich erstellt." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = $"Es ist ein Fehler aufgetreten: {ex.Message}" };
            }
        }
        public async Task<ValidationResult> UpdateGroupProduct(GroupProducts editedGroupProducts)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/GroupProducts/updateGroupProduct", editedGroupProducts);
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Die Produktgruppe konnte nicht aktualisiert werden.." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = result?.Message ?? "Die Produktgruppe konnte nicht aktualisiert werden.." };
                }
                // Update local list
                var index = DownloadedproductGroups.FindIndex(gp => gp.Id == editedGroupProducts.Id);
                if (index != -1)
                {
                    DownloadedproductGroups[index] = editedGroupProducts;
                }
                return new ValidationResult { Result = true, Message = "Produktgruppe erfolgreich aktualisiert." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = $"Es ist ein Fehler aufgetreten: {ex.Message}" };
            }
        }
        public async Task<ValidationResult> DeleteGroupProducts(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/GroupProducts/deleteGroupProducts/{id}");
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                {
                    return result ?? new ValidationResult { Result = false,Message = "Die Produktgruppe konnte nicht gelöscht werden.." };
                }


                if (result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Die Produktgruppe konnte nicht gelöscht werden.." };
                }

                // Remove from local list
                DownloadedproductGroups.RemoveAll(gp => gp.Id == id);
                return new ValidationResult{ Result = true,Message = "Produktgruppe erfolgreich gelöscht." };
            }
            catch (Exception ex)
            {
                return new ValidationResult {Result = false,  Message = $"Es ist ein Fehler aufgetreten: {ex.Message}"  };
            }
        }
        // Local
        public void AddCategoriesToLocal(List<GroupProducts> groupProducts)
        {
            if (groupProducts.Count > 0 && DownloadedproductGroups.Count == 0)
            {
                DownloadedproductGroups.AddRange(groupProducts);
                return;
            }

            foreach (var group in groupProducts)
            {
                if (!DownloadedproductGroups.Any(p => p.Id == group.Id))
                {
                    DownloadedproductGroups.Add(group);
                }
            }
        }
        public void AddCategoriesToLocal(GroupProducts groupProducts)
        {
            if (!DownloadedproductGroups.Any(p => p.Id == groupProducts.Id))
            {
                DownloadedproductGroups.Add(groupProducts);
            }
        }
        public bool IsCategoryEdited(GroupProducts currentGroup, GroupProducts editGroup)
        {
            if (currentGroup.GroupName_de != editGroup.GroupName_de ||
                currentGroup.GroupName_ar != editGroup.GroupName_ar)
            {
                return true;
            }
            return false;
        }
    }
}
