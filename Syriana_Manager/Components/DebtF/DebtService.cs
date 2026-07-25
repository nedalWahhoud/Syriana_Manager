using Syriana_Manager.Components.Model;

namespace Syriana_Manager.Components.DebtF
{
    public class DebtService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<DebtCustomers> GetDebtCustomerByIdAsync(int customerId)
        {
            try
            {
                var response = await _http.GetAsync($"api/DebtCustomers/getDebtByCustomerId/{customerId}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var debtCustomer = await response.Content.ReadFromJsonAsync<DebtCustomers>();
                return debtCustomer!;
            }
            catch (Exception)
            {
                return null!;
            }
        }
    }
}
