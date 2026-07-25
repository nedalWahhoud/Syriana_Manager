using Syriana_Manager.Components.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Syriana_Manager.Components.TransactionsCustomersF
{
    public class TransactionsCustomersService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public GetItems<TransactionsCustomers> GetItems { get; set; } = new();

        public List<TransactionsCustomers> DownloadedTransactionsCustomers { get; set; } = [];

        public async Task<ValidationResult> AddTransaction(TransactionsCustomers transaction)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/TransactionsCustomers/addTransaction", transaction);

                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannte Fehler." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (result != null && result.Result == true)
                {
                    // -100 ist ein spezieller Wert, der anzeigt, dass die Transaktion erfolgreich hinzugefügt wurde, aber die ID nicht zurückgegeben werden kann, da sie von einem Trigger alle Transaktons gelöscht wurden, weil die Balance == 0.
                    if (result.NewId != -100)
                    {
                        transaction = await GetTransactionsCustomerByIdAsync(result.NewId!.Value);
                        if (transaction == null)
                            return new ValidationResult { Result = false, Message = "Transaktion konnte nicht gefunden werden." };

                        AddToLocal(transaction, 0);
                        return result;
                    }
                    else
                    {
                        // Lokalen Cache leeren und Transaktionen neu laden
                        DownloadedTransactionsCustomers.Clear();
                        // Alle Transaktionen abrufen, um den lokalen Cache zu aktualisieren
                        await GetTransactionsCustomersAsync();
                        return result;
                    }
                }
                else
                    return result ?? new ValidationResult { Result = false, Message = "Unbekannte Fehler." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = $"Es ist ein Fehler aufgetreten: {ex.Message}" };
            }
        }
        public async Task<ValidationResult> GetTransactionsCustomersAsync()
        {

            if (GetItems.AllItemsLoaded)
                return new ValidationResult() { Result = true, Message = string.Empty };

            try
            {
                var response = await _http.GetAsync($"api/TransactionsCustomers/getTransactionsCustomers?CurrentPage={GetItems.CurrentPage}&PageSize={GetItems.PageSize}&AllItemsLoaded={GetItems.AllItemsLoaded}&Filter.Id={GetItems.Filter?.Id}" +
                    $"&Filter.Type={(int)(GetItems.Filter?.Type ?? GetItemFilterType.None)}");
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult() { Result = false, Message = "" };
                }

                var result = await response.Content.ReadFromJsonAsync<GetItems<TransactionsCustomers>>();
                if (result == null)
                    return new ValidationResult() { Result = false, Message = "" };
                else
                {
                    GetItems.CurrentPage = result.CurrentPage;
                    GetItems.AllItemsLoaded = result.AllItemsLoaded;

                    AddToLocal(result.Items);
                    return new ValidationResult() { Result = true, Message = "" };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult() { Result = false, Message = ex.Message };
            }
        }
        public async Task<TransactionsCustomers> GetTransactionsCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/TransactionsCustomers/getTransactionsCustomerById?id={id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var result = await response.Content.ReadFromJsonAsync<TransactionsCustomers>();
                if (result == null)
                    return null!;
                else
                    return result;
            }
            catch
            {
                return null!;
            }
        }
        // local
        public void AddToLocal(List<TransactionsCustomers> transactionsCustomers)
        {
            if (transactionsCustomers.Count > 0 && DownloadedTransactionsCustomers.Count == 0)
            {
                DownloadedTransactionsCustomers.AddRange(transactionsCustomers);
                return;
            }
            foreach (var transactionsCustomer in transactionsCustomers)
            {
                if (!DownloadedTransactionsCustomers.Any(p => p.Id == transactionsCustomer.Id))
                {
                    DownloadedTransactionsCustomers.Add(transactionsCustomer);
                }
            }
        }
        public void AddToLocal(TransactionsCustomers transactionsCustomer, int index)
        {
            if (!DownloadedTransactionsCustomers.Any(p => p.Id == transactionsCustomer.Id))
            {
                DownloadedTransactionsCustomers.Insert(index, transactionsCustomer);
            }
        }

        public string GetCustomerNotificationToken(int customerId)
        {
            var claims = new[]  {new Claim(ClaimTypes.NameIdentifier, customerId.ToString()),new Claim("type", "transaction") };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtSettings.Key!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: JwtSettings.Issuer,
                audience: JwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
