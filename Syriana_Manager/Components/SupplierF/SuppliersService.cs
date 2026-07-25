using Syriana_Manager.Components.Model;

namespace Syriana_Manager.Components.SupplierF
{
    public class SuppliersService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public List<Suppliers> DownloadedSuppliers { get; set; } = [];
        public async Task<List<Suppliers>> GetAllSuppliers()
        {
            if (DownloadedSuppliers.Count > 0)
                return DownloadedSuppliers;
            try
            {
                var response = await _http.GetAsync($"api/Suppliers/getSuppliers");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Suppliers>>();

                // add the manufacturers to the local list
                AddProductToLocal(getItems?.Items ?? []);

                return DownloadedSuppliers;
            }
            catch
            {
                return [];
            }
        }
        public async Task <Suppliers> GetSupplierByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Suppliers/getSupplierById/{id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var supplier = await response.Content.ReadFromJsonAsync<Suppliers>();
                if(supplier == null)
                    return null!;

                return supplier;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<ValidationResult> AddSupplier(Suppliers supplier)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/Suppliers/addSupplier", supplier);
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                    return result ?? new ValidationResult { Result = false, Message = "Lieferanten konnten nicht hinzugefügt werden." };
                if (result != null && result.Result)
                {
                    supplier.Id = result.NewId ?? 0; // Setze die ID des neuen Lieferanten basierend auf der Antwort des Servers
                    // neuen Lieferanten zur lokalen Liste hinzufügen.
                    AddProductToLocal(supplier);
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unerwarteter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateSupplier(Suppliers supplier)
        {
            Suppliers? currentSupplier = DownloadedSuppliers.FirstOrDefault(s => s.Id == supplier.Id);

            if (currentSupplier == null)
                return new ValidationResult { Result = false, Message = "Lieferant nicht gefunden." };

            bool isEdited = IsEdited(currentSupplier, supplier);
            if (!isEdited)
                return new ValidationResult { Result = false, Message = "Keine Änderungen erkannt." };

            try
            {
                var response = await _http.PutAsJsonAsync($"api/Suppliers/updateSupplier", supplier);
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Lieferanten konnten nicht aktualisiert werden." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result != null && result.Result)
                {
                    // Lieferanten in der lokalen Liste aktualisieren
                    var index = DownloadedSuppliers.FindIndex(s => s.Id == supplier.Id);
                    if (index != -1)
                    {
                        DownloadedSuppliers[index] = supplier;
                    }
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unerwarteter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteSupplier(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Suppliers/deleteSupplier/{id}");
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                    return result ?? new ValidationResult { Result = false, Message = "Lieferanten konnten nicht gelöscht werden." };
                if (result != null && result.Result)
                {
                    // Lieferanten aus der lokalen Liste entfernen
                    DownloadedSuppliers.RemoveAll(s => s.Id == id);
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unerwarteter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // local
        public void AddProductToLocal(List<Suppliers> suppliers)
        {
            if (suppliers.Count > 0 && DownloadedSuppliers.Count == 0)
            {
                DownloadedSuppliers.AddRange(suppliers);
                return;
            }
            foreach (var supplier in suppliers)
            {
                if (!DownloadedSuppliers.Any(p => p.Id == supplier.Id))
                {
                    DownloadedSuppliers.Add(supplier);
                }
            }
        }
        public void AddProductToLocal(Suppliers supplier)
        {
            if (!DownloadedSuppliers.Any(p => p.Id == supplier.Id))
            {
                DownloadedSuppliers.Add(supplier);
            }
        }
        public Suppliers GetSupplierByIdLocal(int id)
        {
            return DownloadedSuppliers.FirstOrDefault(s => s.Id == id) ?? null!;
        }
        private bool IsEdited(Suppliers currentSupplier, Suppliers editSupplier) => currentSupplier.Name != editSupplier.Name ||
                   currentSupplier.Street != editSupplier.Street ||
                   currentSupplier.HNumber != editSupplier.HNumber ||
                   currentSupplier.PostalCode != editSupplier.PostalCode ||
                   currentSupplier.City != editSupplier.City ||
                   currentSupplier.Country != editSupplier.Country ||
                   currentSupplier.Phone != editSupplier.Phone ||
                   currentSupplier.Email != editSupplier.Email ||
                   currentSupplier.Website != editSupplier.Website;

    }
}
