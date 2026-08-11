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


                if(response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return null!;

                if (response.IsSuccessStatusCode)
                {
                    var debtCustomer = await response.Content.ReadFromJsonAsync<DebtCustomers>();
                    return debtCustomer!;
                }

                return null!;

            }
            catch (Exception)
            {
                return null!;
            }
        }
    }
}
