using Syriana_Manager.Components.CustomersF;
using Syriana_Manager.Components.DistributionLinesF;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Syriana_Manager;
using Syriana_Manager.Components.CartF;
using Syriana_Manager.Components.CategoriesF;
using Syriana_Manager.Components.DiscountF;
using Syriana_Manager.Components.ImagesF;
using Syriana_Manager.Components.InvoiceF;
using Syriana_Manager.Components.LocalStorageF;
using Syriana_Manager.Components.LogIn;
using Syriana_Manager.Components.OrderF;
using Syriana_Manager.Components.ProductGroupF;
using Syriana_Manager.Components.ProductsF;
using Syriana_Manager.Components.SupplierF;
using Syriana_Manager.Components.TaxRatesF;
using Syriana_Manager.Components.TransactionsCustomersF;
using Syriana_Manager.Components.Share;
using Syriana_Manager.Components.DebtF;
using Syriana_Manager.Components.OneTimePaymentsF;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");



// http api
builder.Services.AddScoped(sp =>
{
    return new HttpClient { BaseAddress = AppConfig.ApiUri };
});



// auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
// auth AuthService
builder.Services.AddScoped<AuthService>();
// products
builder.Services.AddScoped<ProductService>();
// Suppliers
builder.Services.AddScoped<SuppliersService>();
// order
builder.Services.AddScoped<OrderService>();
// cart
builder.Services.AddScoped<CartService>();
// addresses
//builder.Services.AddScoped<AddressService>();
// Receipt
//builder.Services.AddScoped<ReceiptService>();
// Group Products
builder.Services.AddScoped<ProductGroupService>();
// discount
builder.Services.AddScoped<DiscountService>();
// categories
builder.Services.AddScoped<CategoryService>();
// Invoice
builder.Services.AddScoped<InvoiceService>();
// ProductImages
builder.Services.AddScoped<ProductImagesService>();
//  Carousel Image 
builder.Services.AddScoped<CarouselImageService>();
//  DistributionLines Service 
builder.Services.AddScoped<DistributionLinesService>();
//  Customers Service 
builder.Services.AddScoped<CustomersService>();
//  WhatsApp Service 
builder.Services.AddScoped<WhatsAppService>();
//  DebtCustomers Service 
builder.Services.AddScoped<DebtService>();
builder.Services.AddScoped<WhatsAppService>();
//  TransactionsCustomersService Service 
builder.Services.AddScoped<TransactionsCustomersService>();
//  oneTimepayment Service 
builder.Services.AddScoped<OneTimePaymentsService>();
// LocalStorageService
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<LocalStorageService>();
// TaxRatesService
builder.Services.AddScoped<TaxRatesService>();

// sprache
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

var app = builder.Build();



var jsRuntime = app.Services.GetRequiredService<IJSRuntime>();
var result = await jsRuntime.InvokeAsync<string>("blazorCulture.get");

string cultureName = !string.IsNullOrEmpty(result) ? result : "de"; 

var culture = new CultureInfo(cultureName);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await app.RunAsync();
