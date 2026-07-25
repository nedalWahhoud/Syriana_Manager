namespace Syriana_Manager.Components.DistributionLinesF
{
    public class DistributionLinesService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public List<DistributionLines> DownloadedDistributionLines { get; private set; } = [];
        public async Task<ValidationResult> GetAllDistributionLinesAsync()
        {
            if(DownloadedDistributionLines.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Bereits abgerufen." };
            }
            try
            {
                var response = await _http.GetAsync("api/DistributionLines/getAllDistributionLines");
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Abrufen." };
                var distributionLines = await response.Content.ReadFromJsonAsync<List<DistributionLines>>();
                if (distributionLines != null)
                {
                    DownloadedDistributionLines = distributionLines;
                    return new ValidationResult { Result = true, Message = "erfolgreich abgerufen." };
                }
                return new ValidationResult { Result = false, Message = "Keine Items gefunden." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<DistributionLines> GetDistributionLineByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/DistributionLines/getDistributionLineById/{id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var distributionLine = await response.Content.ReadFromJsonAsync<DistributionLines>();
                if(distributionLine != null)
                {
                    AddToLocal(distributionLine);
                }

                return distributionLine!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<ValidationResult> AddDistributionLineAsync(DistributionLines newDistributionLine)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/DistributionLines/addDistributionLine", newDistributionLine);
                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Hinzufügen der Verteilerzeile." };
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result?.Result == true)
                {
                    if (result != null && result.Result)
                    {
                        newDistributionLine.Id = result.NewId ?? 0;
                        AddToLocal(newDistributionLine);
                    }
                }
                return result ?? new ValidationResult { Result = false, Message = "Es ist ein unbekannter Fehler aufgetreten." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateDistributionLineAsync(DistributionLines updatedDistributionLine)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/DistributionLines/updateDistributionLine", updatedDistributionLine);
                if (!response.IsSuccessStatusCode)
                    return new ValidationResult { Result = false, Message = "Fehler beim Aktualisieren." };

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                {
                    return new ValidationResult { Result = false, Message = result?.Message ?? "Die Produktgruppe konnte nicht aktualisiert werden.." };
                }

                if (result.Result)
                {
                    // Update local list
                    var index = DownloadedDistributionLines.FindIndex(gp => gp.Id == updatedDistributionLine.Id);
                    if (index != -1)
                    {
                        DownloadedDistributionLines[index] = updatedDistributionLine;
                    }
                }

                return new ValidationResult { Result = true, Message = "Produktgruppe erfolgreich aktualisiert." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteDistributionLineAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/DistributionLines/deleteDistributionLine/{id}");

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode)
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Löschen der Verteilerzeile." };
               
                if (result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = result?.Message ?? "Die Verteilerzeile konnte nicht gelöscht werden." };
                }
                // Remove from local list
                var index = DownloadedDistributionLines.FindIndex(gp => gp.Id == id);
                if (index != -1)
                {
                    DownloadedDistributionLines.RemoveAt(index);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // local
        public DistributionLines GetDistributionLineByIdLocal(int id)
        {
            var distributionLine = DownloadedDistributionLines.FirstOrDefault(p => p.Id == id);
            if (distributionLine != null)
            {
                return distributionLine;
            }
            return null!;
        }
        public void AddToLocal(DistributionLines distributionLine)
        {
            if (!DownloadedDistributionLines.Any(p => p.Id == distributionLine.Id))
            {
                DownloadedDistributionLines.Add(distributionLine);
            }
        }
        public void AddToLocal(List<DistributionLines> distributionLines)
        {
            if (distributionLines.Count > 0 && distributionLines.Count == 0)
            {
                DownloadedDistributionLines.AddRange(distributionLines);
                return;
            }

            foreach (var DistributionLine in DownloadedDistributionLines)
            {
                if (!DownloadedDistributionLines.Any(p => p.Id == DistributionLine.Id))
                {
                    DownloadedDistributionLines.Add(DistributionLine);
                }
            }
        }
        public bool IsEdited(DistributionLines currentDistributionLine, DistributionLines editDistributionLine)
        {
            return currentDistributionLine.LineName != editDistributionLine.LineName ||
                   currentDistributionLine.Description != editDistributionLine.Description;
        }
    }
}
