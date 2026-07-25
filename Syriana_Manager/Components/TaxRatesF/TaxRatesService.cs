using Syriana_Manager.Components.Model;

namespace Syriana_Manager.Components.TaxRatesF
{
    public class TaxRatesService(HttpClient http)
    {
        private readonly HttpClient _http = http;

        public List<TaxRate> DownloadedTaxRates { get; set; } = [];

        public async Task<List<TaxRate>> GetAllTaxRates()
        {
            if (DownloadedTaxRates.Count > 0)
                return DownloadedTaxRates;
            try
            {
                var response = await _http.GetAsync($"api/TaxRates/getTaxRates");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<TaxRate>>();
                // add the tax rates to the local list
                DownloadedTaxRates.AddRange(getItems?.Items ?? []);

                return DownloadedTaxRates;
            }
            catch
            {
                return [];
            }
        }

        public async Task<TaxRate> GetTaxRateByIdAsync(int TaxId)
        {
            try
            {
                var response = await _http.GetAsync($"api/TaxRates/getTaxRateById/{TaxId}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var taxRate = await response.Content.ReadFromJsonAsync<TaxRate>();

                if (taxRate == null)
                    return null!;

                AddTaxRateToLocal(taxRate);
                return taxRate;
            }
            catch
            {
                return null!;
            }
        }

        public TaxRate GetTasRateLocal(int taxId)
        {
            var taxRate = DownloadedTaxRates.Find(t => t.Id == taxId);
            if (taxRate != null)
                return taxRate;
            else
            {
                return null!;
            }
        }
        public void AddTaxRateToLocal(List<TaxRate> taxRates)
        {
            if (taxRates.Count > 0 && DownloadedTaxRates.Count == 0)
            {
                DownloadedTaxRates.AddRange(taxRates);
                return;
            }
            foreach (var supplier in taxRates)
            {
                if (!DownloadedTaxRates.Any(p => p.Id == supplier.Id))
                {
                    DownloadedTaxRates.Add(supplier);
                }
            }
        }
        public void AddTaxRateToLocal(TaxRate taxRate)
        {
            if (!DownloadedTaxRates.Any(p => p.Id == taxRate.Id))
            {
                DownloadedTaxRates.Add(taxRate);
            }
        }
    }
}
