using Syriana_Manager.Components.Model; 
namespace Syriana_Manager.Components.ProductsF
{
    public class ProductService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<Products> DownloadedProduct { get; set; } = [];

        public async Task<List<Products>> GetProductByIdsAsync(List<int> productIds)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/getProductByIds", productIds);

                if (!response.IsSuccessStatusCode)
                    return null!;
                var products = await response.Content.ReadFromJsonAsync<List<Products>>();
                if (products != null)
                {
                    // add the product to the local list
                    AddProductToLocal(products);
                    return products;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<GetItems<Products>> GetProducts(GetItems<Products> getItem)
        {
            if (getItem.AllItemsLoaded)
                return new() { AllItemsLoaded = getItem.AllItemsLoaded };
            try
            {
                var response = await _http.PostAsJsonAsync($"api/Products/getProducts", getItem);

                if (!response.IsSuccessStatusCode)
                    return new();

                getItem = await response.Content.ReadFromJsonAsync<GetItems<Products>>() ?? new();

                // add to local list
                AddProductToLocal(getItem.Items);

                if (getItem.AllItemsLoaded == true)
                {
                    return getItem;
                }
                else
                {
                    getItem.CurrentPage++;
                    return getItem;
                }

            }
            catch
            {
                return new();
            }
        }

        public async Task<ValidationResult> AddProductAsync(Products newProduct)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/addProduct", newProduct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannte error." };
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (result?.Result == true && result.NewId != null)
                    return result;
                else
                    return new ValidationResult { Result = false, Message = "Unbekannte Fehler." };

            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteProductAsync(int productId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/Products/deleteProduct/{productId}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." }; ;
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                return result!;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateProductAsync(Products editProduct)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/updateProduct", editProduct);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler aufgetreten." };
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (result?.Result == true)
                {
                    return result;
                }

                return result ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<Products> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getProductById/{productId}");

                if (!response.IsSuccessStatusCode)
                    return null!;
                var product = await response.Content.ReadFromJsonAsync<Products>();
                if (product != null)
                {
                    return product!;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }

        public bool IsEditedProduct(Products currentProduct, Products editProduct)
        {
            if (editProduct == null || currentProduct == null)
            {
                return false;
            }
            else
            {
                // Initialisiere die SelectedSupplierIds aus der Lieferantensammlung
                currentProduct.SelectedSupplierIds = currentProduct.Suppliers.Select(s => s.Id).ToList();
            }

            if (currentProduct.Name_de != editProduct.Name_de)
                return true;
            if (currentProduct.Description_de != editProduct.Description_de)
                return true;
            if (currentProduct.Name_ar != editProduct.Name_ar)
                return true;
            if (currentProduct.Description_ar != editProduct.Description_ar)
                return true;
            if (currentProduct.CategoryId != editProduct.CategoryId)
                return true;
            if (currentProduct.Barcode != editProduct.Barcode)
                return true;
            if (currentProduct.Quantity != editProduct.Quantity)
                return true;
            if (currentProduct.PurchasePrice != editProduct.PurchasePrice)
                return true;
            if (currentProduct.SalePrice != editProduct.SalePrice)
                return true;
            if (currentProduct.MinimumStock != editProduct.MinimumStock)
                return true;
            if (currentProduct.EXPDate != editProduct.EXPDate)
                return true;
            if(currentProduct.PackagingUnit != editProduct.PackagingUnit)
                return true;
            if(currentProduct.ItemsPerPackage != editProduct.ItemsPerPackage)
                return true;
            var currentIds = currentProduct.SelectedSupplierIds ?? [];
            var editIds = editProduct.SelectedSupplierIds ?? [];

            bool isSuppliersChanged = currentIds.Count != editIds.Count ||
                                       editIds.Except(currentIds).Any() ||
                                       currentIds.Except(editIds).Any();
            if (isSuppliersChanged)
                return true;
            if (currentProduct.TaxRateId != editProduct.TaxRateId)
                return true;
            if (currentProduct.ProductGroupID != editProduct.ProductGroupID)
                return true;
            if (currentProduct.IsShippable != editProduct.IsShippable)
                return true;
            if (HasDiscountChanged(currentProduct.ProductDiscount, editProduct.ProductDiscount))
                return true;
            if (currentProduct.ProductImages.FirstOrDefault(i => i.IsMain)?.LastModified != editProduct.ProductImages.FirstOrDefault(i => i.IsMain)?.LastModified)
                return true;

            return false;
        }
        private static bool HasDiscountChanged(ProductDiscounts? oldDiscount, ProductDiscounts? newDiscount)
        {
            var effectiveOld = oldDiscount == null || oldDiscount.DiscountedPrice <= 0 ? null : oldDiscount;
            var effectiveNew = newDiscount == null || newDiscount.DiscountedPrice <= 0 ? null : newDiscount;


            if (effectiveOld == null && effectiveNew == null)
                return false;

            if (effectiveOld == null || effectiveNew == null)
                return true;

            return effectiveOld.DiscountedPrice != effectiveNew.DiscountedPrice ||
                   effectiveOld.StartDate != effectiveNew.StartDate ||
                   effectiveOld.EndDate != effectiveNew.EndDate;
        }
        public ValidationResult IsValidProduct(Products newProduct)
        {
            if (string.IsNullOrWhiteSpace(newProduct.Name_de))
            {
                return new ValidationResult() { NewId = null, Result = false, Message = "Die Angabe des Produktnamens ist erforderlich." };
            }

            if (string.IsNullOrWhiteSpace(newProduct.Description_de))
            {
                return new ValidationResult() { Result = false, Message = "Die Angabe der Produktbeschreibung ist erforderlich." };
            }
            if (string.IsNullOrWhiteSpace(newProduct.Name_ar))
            {
                return new ValidationResult() { Result = false, Message = "Die Angabe des Produktnamens_ar ist erforderlich." };
            }

            if (string.IsNullOrWhiteSpace(newProduct.Description_ar))
            {
                return new ValidationResult() { Result = false, Message = "Die Angabe der Produktbeschreibung_ar ist erforderlich." };

            }

            if (newProduct!.CategoryId <= 0)
            {
                return new ValidationResult() { Result = false, Message = "Eine Kategorie ist erforderlich." };
            }
            if (newProduct.Quantity < 0)
            {
                return new ValidationResult() { Result = false, Message = "Die Menge muss größer als -1 sein." };
            }
            if (newProduct.SalePrice <= 0)
            {
                return new ValidationResult() { Result = false, Message = "Der Verkaufspreis muss größer als 0 sein." };
            }
            if (newProduct.PurchasePrice <= 0)
            {
                return new ValidationResult() { Result = false, Message = "Der Einkaufspreis muss größer als 0 sein." };
            }
            if (newProduct.MinimumStock < 0)
            {
                return new ValidationResult() { Result = false, Message = "Der Mindestbestand muss größer als -1 sein." };
            }
            if (newProduct.SelectedSupplierIds == null || newProduct.SelectedSupplierIds.Count == 0)
            {
                return new ValidationResult() { Result = false, Message = "Ein Hersteller ist erforderlich." };
            }
            if (newProduct.TaxRateId <= 0)
            {
                return new ValidationResult() { Result = false, Message = "Ein Steuersatz ist erforderlich." };
            }
            /* if (newProduct.EXPDate < DateTime.Now)
             {
                 return new ValidationResult() {Result = false, Message = "Das Ablaufdatum muss größer als das aktuelle Datum sein." };
             }*/
            foreach (var item in newProduct.ProductImages)
            {

                if (item.ImageBytes == null || item.ImageBytes != null && item.ImageBytes.Length <= 0)
                {
                    return new ValidationResult() { Result = false, Message = "Das Hauptbild ist erforderlich." };
                }
            }
            return new ValidationResult() { Result = true, Message = string.Empty };
        }
        //
        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/Orders/getPaymentMethods");
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
                return await response.Content.ReadFromJsonAsync<List<PaymentMethod>>() ?? [];

            }
            catch
            {
                return [];
            }
        }
        // local
        public void AddProductToLocal(List<Products> products)
        {
            if (products.Count > 0 && DownloadedProduct.Count == 0)
            {
                DownloadedProduct.AddRange(products);
                return;
            }
            foreach (var product in products)
            {
                if (!DownloadedProduct.Any(p => p.Id == product.Id))
                {
                    DownloadedProduct.Add(product);
                }
            }
        }
        public void AddProductToLocal(Products product)
        {
            if (!DownloadedProduct.Any(p => p.Id == product.Id))
            {
                DownloadedProduct.Add(product);
            }
        }
        public List<Products> GetProductByCategoryIdLocal(int categoryId)
        {
            return [.. DownloadedProduct
                    .Where(p => p.CategoryId == categoryId)
                    .OrderBy(p => p.Name_de)];
        }
        public Products GetProductByIdLocal(int productId)
        {
            var product = DownloadedProduct.Find(p => p.Id == productId);
            if (product != null)
                return product;
            else
            {
                return null!;
            }
        }
       
        // barcode
        public async Task<ValidationResult> UpdateBarcodeAsync(int id, string barcode)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/products/updateBarcode/{id}", barcode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResult = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return errorResult ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler aufgetreten." };
                }


                var result = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler aufgetreten." };

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    Result = false,
                    Message = $"Verbindung zum Server fehlgeschlagen: {ex.Message}"
                };
            }
        }
    }
}
