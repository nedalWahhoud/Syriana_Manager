using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syriana_Manager.Components.LocalStorageF;
using Syriana_Manager.Components.Model;
using Syriana_Manager.Components.Share;

namespace Syriana_Manager.Components.CustomersF
{
    public class CustomerDropdownService(WhatsAppService whatsAppService, IJSRuntime jSRuntime,LocalStorageService localStorageService,NavigationManager navigationManager) 
    {
        private readonly WhatsAppService _whatsAppService = whatsAppService;
        private readonly IJSRuntime _jSRuntime = jSRuntime;
        private readonly LocalStorageService _localStorageService = localStorageService;
        private readonly NavigationManager _navigationManager = navigationManager;

        public async Task OnDebts(Customers customer)
        {
            if (customer != null)
            {
                await _localStorageService.Set("selectedStopNummerId", customer.StopNumber);
                _navigationManager.NavigateTo($"/debt/{customer.Id}");
            }
        }
        public async Task OnMap(Customers customer)
        {
            try
            {
                var (isValidCoordinates, hasAddress, fullAddress) = CustomersService.ValidateAndBuildMapAddress(customer);
                if (!isValidCoordinates && !hasAddress)
                {
                    await _jSRuntime.InvokeVoidAsync("alert", "Keine gültigen Koordinaten oder Adresse für diesen Kunden vorhanden.");
                    return;
                }

                await _jSRuntime.InvokeVoidAsync("mapRedirect.openMap", customer.Latitude, customer.Longitude, fullAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnMap Fehler CustomDropdown{ex.InnerException?.Message ?? ex.Message}");
            }
        }
        public async Task OnPhone(string? phoneNumber)
        {
            try
            {
                if (phoneNumber == null || string.IsNullOrWhiteSpace(phoneNumber))
                {
                    await _jSRuntime.InvokeVoidAsync("alert", "Keine Telefonnummer für diesen Kunden vorhanden.");
                    return;
                }

                await _jSRuntime.InvokeVoidAsync("phoneRedirect.openPhoneDialer", phoneNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error redirecting to phone: {ex.Message}");
            }
        }
        public async Task OnWhatsAppMessage(Customers customer)
        {
            var result = await _whatsAppService.SendMassage(customer);
            if (!result.Result)
            {
                await _jSRuntime.InvokeVoidAsync("alert", result.Message);
            }
        }
        public async Task viaWhatasappShare(Customers customer)
        {
            var result = await _whatsAppService.SendCustomerInfo(customer);
            if (!result.Result)
            {
                await _jSRuntime.InvokeVoidAsync("alert", result.Message);
            }
        }

    }
}
