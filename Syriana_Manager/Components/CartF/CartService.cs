using Syriana_Manager.Components.ProductsF;
using Syriana_Manager.Components.Model;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Syriana_Manager.Components.CartF
{
    public class CartService(IJSRuntime js, ProductService productService)
    {
        public List<CartItem> CartItems { get; private set; } = [];
        public event Action? OnChange;
        private readonly IJSRuntime _js = js;
        private readonly ProductService _productService = productService;

        // event to change the cart state
        private void NotifyStateChanged() => OnChange?.Invoke();
        public async Task InitializeAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "cart");
                //
                var items = (json == null ? [] : JsonSerializer.Deserialize<List<CartItem>>(json));
                CartItems = items ?? [];
                NotifyStateChanged();
            }
            catch
            {
                CartItems = [];
            }
        }
        public async Task<string> AddToCart(int productId)
        {
            Products product = _productService.GetProductByIdLocal(productId) ?? await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return "Produkt nicht gefunden";
            //
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                // Update the existing item quantity
                item.Quantity++;

                // check if has stock
                if (!HasStock(item.Quantity, product.Quantity))
                {
                    item.Quantity--; // revert the quantity increase
                    return "Nicht genügend Lagerbestand";
                }


                if (item.Quantity > 5)
                {
                    item.Quantity = 5;
                    return "Sie können nicht mehr als 5 Stück bestellen.";
                }
            }
            // hier wird die quantität nicht überprüft, da es sich um ein neues produkt handelt und die quantität sollte mindestens 1 sein, um zu benutzer zu zeigen
            else
            {
                if(product.Quantity <= 0)
                    return "Nicht genügend Lagerbestand";
                // add the new item to the genrall cart
                CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1, // add first item with quantity 1
                    Product = _productService.GetProductByIdLocal(productId) ?? await _productService.GetProductByIdAsync(productId),
                });
            }
            await SaveCart(); // Save the updated cart to local storage
            NotifyStateChanged();
            return null!;
        }
        public async Task<string> DecreaseFromCart(int productId)
        {
            if (IsQuantityZero(productId))
            {
                return null!; // Cannot decrease below zero
            }

            else
            {
                var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (item != null)
                {

                    item.Quantity--;

                    if (item.Quantity == 0)
                    {
                        RemoveFromCart(productId);
                    }

                    await SaveCart();
                    NotifyStateChanged();
                    return null!;
                }
                return "Nicht in einkaufswagen gefunden"; // Product not found in the cart
            }
        }
        public async void RemoveFromCart(int ProductId)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == ProductId);
            if (item != null)
            {
                CartItems.Remove(item);
                await SaveCart();
                NotifyStateChanged();
            }
        }
        public bool HasStock(int requestedQuantity, int minimumStock)
        {
            return minimumStock >= requestedQuantity;
        }
        private async Task SaveCart()
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "cart", JsonSerializer.Serialize(CartItems));
        }

        //
        public int GetTotalQuantity()
        {
            return CartItems.Sum(ci => ci.Quantity);
        }
        public int GetQuantityOfProduct(int productId)
        {
            return CartItems.FirstOrDefault(c => c.ProductId == productId)?.Quantity ?? 0;
        }
        public double GetTotalPrice()
        {
            return (double)(CartItems.Sum(ci => ci.Product?.SalePrice * ci.Quantity) ?? 0);
        }
        public bool IsProductAdded(int productId)
        {
            return CartItems.Any(ci => ci.ProductId == productId);
        }
        public CartItem GetCartItemByProductId(int productId)
        {
            return CartItems.Find(ci => ci.ProductId == productId) ?? null!;
        }
        public async Task<ValidationResult> LoadCartProductsAsync()
        {
            List<int> idsUnlocalProducts = [];
            for (int i = 0; i < CartItems.Count; ++i)
            {
                if (CartItems[i].Product == null)
                {
                    var product = _productService.GetProductByIdLocal(CartItems[i].ProductId);
                    if (product != null)
                        CartItems[i].Product = product;
                    else
                        // if no local product found, add to idsUnlocalProducts to fetch from server
                        idsUnlocalProducts.Add(CartItems[i].ProductId);
                }
            }
            // Fetch products from server for those not found locally and update cart items
            if (idsUnlocalProducts.Count > 0)
            {
                var products = await _productService.GetProductByIdsAsync(idsUnlocalProducts);
                if (products != null && products.Count > 0)
                {
                    foreach (var product in products)
                    {
                        var cartItem = CartItems.FirstOrDefault(ci => ci.ProductId == product.Id);
                        if (cartItem != null)
                        {
                            cartItem.Product = product;
                        }
                    }
                }
                else
                    return new ValidationResult() { Result = false, Message = "Failed to retrieve products from server." };

            }

            return new ValidationResult() { Result = true };
        }
        public async void ClearCart(List<Products>? products = null)
        {
            if (products != null && products.Count > 0 && CartItems.Count > 0)
            {
                var productIdsInCart = CartItems.Select(ci => ci.ProductId).ToHashSet();

                foreach (var product in products)
                {
                    if (productIdsInCart.Contains(product.Id))
                    {
                        // nach reinigung von cartitems soll die quantity from alle Products 0 wiederstellen
                        product.CartItem.Quantity = 0;
                    }
                }
            }

            CartItems.Clear();
            await SaveCart();
            NotifyStateChanged();
        }

        public void AddToCart(CartItem cartItem)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == cartItem.ProductId);
            if (item != null)
            {
                item.Quantity = cartItem.Quantity;
            }
            else
            {
                CartItems.Add(new CartItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Product = cartItem.Product
                });
            }
        }
        // 
        public double GetTotalTax()
        {
            return (double)Math.Round(CartItems.Sum(item => item.Product.SalePrice * (item.Product.TaxRate!.Rate / 100) * item.Quantity), 3);
        }
        public List<TaxRates> GetTaxRateGroups()
        {
            var taxRateGroups = CartItems
           .GroupBy(ci => ci.Product.TaxRate!.Rate)
           .Select(g =>
           {

               double taxAmount = Math.Round(g.Sum(ci => ci.Product.SalePrice * (ci.Product.TaxRate!.Rate / 100) * ci.Quantity), 3);
               double total = Math.Round(g.Sum(ci => ci.Quantity * ci.Product.SalePrice), 3);
               double netto = total - taxAmount;

               return new TaxRates
               {
                   TaxRate = g.Key,
                   NettoPrice = netto,
                   TaxAmount = taxAmount,
                   TotalPrice = total
               };
           })
           .ToList();


            return taxRateGroups;
        }
        public bool IsQuantityZero(int productId)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null)
                return true; // Item not found, treat as zero quantity

            if (item.Quantity <= 0)
            {
                bool isProductAdded = IsProductAdded(item.ProductId);
                if (isProductAdded)
                    RemoveFromCart(item.ProductId);

                return true; // Do not add to cart if quantity is zero
            }
            return false; // Quantity is greater than zero, proceed with adding to cart
        }
    }
}
