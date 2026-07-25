using Syriana_Manager.Components.CustomersF;

namespace Syriana_Manager.Components.OneTimePaymentsF
{
    public class OneTimePaymentsService(HttpClient http,CustomersService customersService)
    {
        private readonly HttpClient _http = http;
        private readonly CustomersService _customersService = customersService;
        public List<(List<OneTimePaymentsGroupDto> Group, int lineId)> DownloadedGroups { get; private set; } = [];
        public async Task<ValidationResult> Add(OneTimePayment newOneTimePayment)
        {
            try
            {
                // löschen der Navigation Properties, da diese nicht übergeben werden sollen
                var paymentToSend = new OneTimePayment
                {
                    Id = newOneTimePayment.Id,
                    CustomerId = newOneTimePayment.CustomerId,
                    DistributionLineId = newOneTimePayment.DistributionLineId,
                    TotalAmount = newOneTimePayment.TotalAmount,
                    PickupDate = newOneTimePayment.PickupDate,
                    AmountCollected = newOneTimePayment.AmountCollected,
                    Status = newOneTimePayment.Status,
                    Notes = newOneTimePayment.Notes,
                    CreatedAt = newOneTimePayment.CreatedAt,
                    Customer = null!,
                    DistributionLine = null!
                };

                var response = await _http.PostAsJsonAsync("api/OneTimePayments/add", paymentToSend);

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Hinzufügen der Einmalzahlung." };
                }

                // add to local
                paymentToSend.Id = result.NewId ?? 0;
                // die Navigation Properties müssen für die Gruppierung in der lokalen Liste gesetzt werden, damit die Anzeige korrekt funktioniert
                paymentToSend.Customer = newOneTimePayment.Customer;
                paymentToSend.DistributionLine = newOneTimePayment.DistributionLine;

                // wenn die line noch nicht von server abgerufen wurde, dann muss nicht in lokale hinzufügen sonst wird die line blockiert, damit wemm der Benutzer zu seite Einamalzahlung geht wird die data von server abgerufen.
                if (DownloadedGroups.Any(g => g.lineId == newOneTimePayment.DistributionLineId))
                {
                    AddOneTimePaymentToLocal(newOneTimePayment.DistributionLineId, null, paymentToSend);
                }

                // prüfen wenn die gelöschte Einmalzahlung heute ist, dann muss die Flag "HasOneTimePaymentToday" in Kundenservice aktualisiert werden, damit die Anzeige in Kundenliste korrekt funktioniert
                if (_customersService.DownloadedCustomers != null && _customersService.DownloadedCustomers.Count > 0)
                {
                    var customer = _customersService.DownloadedCustomers.FirstOrDefault(c => c.Id == paymentToSend?.CustomerId);
                    if (customer != null)
                    {
                        if (!customer.HasOneTimePaymentToday)
                        {
                            if (paymentToSend?.PickupDate.Date == DateTime.Now.Date)
                            {
                                customer.HasOneTimePaymentToday = true;
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> GetGroupedPaymentsByLineId(int lineId)
        {
            try
            {
                if (DownloadedGroups.Any(g => g.lineId == lineId))
                {
                    return new ValidationResult { Result = true, Message = "Einmalzahlungen bereits lokal vorhanden." };
                }

                var response = await _http.GetAsync($"api/OneTimePayments/getGroupedPaymentsByLineId/{lineId}");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Fehler beim Abrufen der Einmalzahlungen." };
                }
                var groupedPayment = await response.Content.ReadFromJsonAsync<List<OneTimePaymentsGroupDto>>();
                if (groupedPayment == null || groupedPayment.Count == 0)
                {
                    return new ValidationResult { Result = false, Message = "Keine Einmalzahlungen für diese Linie." };
                }

                // add to Local
                AddOneTimePaymentToLocal(lineId, groupedPayment);

                return new ValidationResult { Result = true, Message = "Einmalzahlungen erfolgreich abgerufen." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> UpdateOneTimePaymentAsync(OneTimePayment editOneTimePayment)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/OneTimePayments/updateStauts", editOneTimePayment);
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Aktualisieren des Zahlungsstatus." };
                }
                // update in locally list
                List<OneTimePaymentsGroupDto> targetLineList = null!;
                OneTimePaymentsGroupDto targetGroup = null!;
                OneTimePayment oldPayment = null!;

                foreach (var (Group, lineId) in DownloadedGroups)
                {
                    foreach (var group in Group)
                    {
                        var payment = group.Payments.FirstOrDefault(p => p.Id == editOneTimePayment.Id);
                        if (payment != null)
                        {
                            targetLineList = Group;
                            targetGroup = group;
                            oldPayment = payment;
                            break;
                        }
                    }
                    if (oldPayment != null) break;
                }

                // 2. Falls der alte Batch gefunden wird, aktualisiere ihn in der Liste.
                if (oldPayment != null && targetGroup != null)
                {
                    var index = targetGroup.Payments.IndexOf(oldPayment);
                    if (index != -1)
                    {
                        // Ersetze die alten Daten durch die aktualisierten Daten vom Server.
                        targetGroup.Payments[index] = editOneTimePayment;
                    }
                }


                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        public async Task<ValidationResult> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/OneTimePayments/{id}");
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Löschen der Einmalzahlung." };
                }

                if (result.Result == true)
                {
                    // delete in locally list
                    List<OneTimePaymentsGroupDto> targetLineList = null!;
                    OneTimePaymentsGroupDto targetGroup = null!;
                    OneTimePayment paymentToRemove = null!;

                    foreach (var (Group, lineId) in DownloadedGroups)
                    {
                        foreach (var group in Group)
                        {
                            var payment = group.Payments.FirstOrDefault(p => p.Id == id);
                            if (payment != null)
                            {
                                targetLineList = Group;
                                targetGroup = group;
                                paymentToRemove = payment;
                                break;
                            }
                        }
                        if (paymentToRemove != null) break;
                    }

                    //  Wenn wir den Batch finden, löschen wir ihn und bereinigen die Struktur.
                    if (paymentToRemove != null)
                    {
                        // Lösche den Batch aus seiner Sammlung
                        targetGroup.Payments.Remove(paymentToRemove);

                        // Wenn die Gruppe vollständig leer ist und keine neuen Beiträge mehr erhält, löschen Sie die Gruppe selbst.
                        if (targetGroup.Payments.Count == 0)
                        {
                            targetLineList.Remove(targetGroup);
                        }
                    }
                    // prüfen wenn die gelöschte Einmalzahlung heute ist, dann muss die Flag "HasOneTimePaymentToday" in Kundenservice aktualisiert werden, damit die Anzeige in Kundenliste korrekt funktioniert
                    if (_customersService.DownloadedCustomers != null && _customersService.DownloadedCustomers.Count > 0)
                    {
                        var customer = _customersService.DownloadedCustomers.FirstOrDefault(c => c.Id == paymentToRemove?.CustomerId);
                        if (customer != null)
                        {
                            if (customer.HasOneTimePaymentToday)
                            {
                                if(paymentToRemove?.PickupDate.Date == DateTime.Now.Date)
                                {
                                    customer.HasOneTimePaymentToday = false;
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        // loacl
        public void AddOneTimePaymentToLocal(int lineId,List<OneTimePaymentsGroupDto>? groupedPayment = null, OneTimePayment? payment = null)
        {
            // add group
            if (groupedPayment != null)
            {
                var existingLine = DownloadedGroups.FirstOrDefault(g => g.lineId == lineId);
                if (existingLine != default)
                {
                    existingLine.Group.AddRange(groupedPayment);
                }
                else
                {
                    DownloadedGroups.Add((groupedPayment, lineId));
                }
            }
            // add payment
            else if (payment != null)
            {
                var existingLine = DownloadedGroups.FirstOrDefault(g => g.lineId == payment.DistributionLineId);
                if (existingLine != default)
                {
                    var existingGroup = existingLine.Group.FirstOrDefault(g => g.GroupPickupDate.Date == payment.PickupDate.Date);
                    if (existingGroup != default)
                    {
                        existingGroup.Payments.Add(payment);
                        // order 
                        existingGroup.Payments = [.. existingGroup.Payments.OrderBy(o => o.Customer?.StopNumber)];
                    }
                    else
                    {
                        var newGroup = new OneTimePaymentsGroupDto
                        {
                            GroupPickupDate = payment.PickupDate,
                            Payments = [payment]
                        };

                        int index = existingLine.Group.FindIndex(g => g.GroupPickupDate > newGroup.GroupPickupDate);

                        if (index == -1)
                        {
                            existingLine.Group.Add(newGroup);
                        }
                        else
                        {
                            existingLine.Group.Insert(index, newGroup);
                        }
                    }
                }
                else
                {
                    DownloadedGroups.Add((new List<OneTimePaymentsGroupDto>
                    {
                        new() {
                            GroupPickupDate = payment.PickupDate.Date,
                            Payments = [payment]
                        }
                    }, payment.DistributionLineId));
                }
            }
        }
        public OneTimePayment? GetOneTimePaymentByIdLocal(int id)
        {
            return DownloadedGroups
                .SelectMany(lineList => lineList.Group)
                .SelectMany(group => group.Payments)
                .FirstOrDefault(p => p.Id == id);
        }
        public ValidationResult ValidateAmountConsistencyAsync(OneTimePayment oneTimePayment)
        {
            if (oneTimePayment.Status == OneTimePaymentStatus.TeilweiseInkassiert && (oneTimePayment.AmountCollected == 0 || oneTimePayment.AmountCollected >= oneTimePayment.TotalAmount))
            {
                return new ValidationResult { Result = false, Message = "Der gesammelte Betrag darf nicht höher, gleich oder 0 sein als der Gesamtbetrag, um den Status auf 'Teilweise Inkassiert' zu setzen." };
            }
            else if (oneTimePayment.Status == OneTimePaymentStatus.Ueberzahlt && oneTimePayment.AmountCollected <= oneTimePayment.TotalAmount)
            {

                return new ValidationResult { Result = false, Message = "Der gesammelte Betrag muss höher sein als der Gesamtbetrag, um den Status auf 'Überzahlt' zu setzen." };
            }

            return new ValidationResult { Result = true };
        }
        public string GetStatusClass(OneTimePaymentStatus status,bool isDropdown, bool isBaseClass = true)
        {
            string dropdownClass = isDropdown ? "dropdown-toggle" : "";

            string baseClass = $"btn btn-sm badge {dropdownClass}";

            string textColor = "text-white";
            string colorClass = status switch
            {
                OneTimePaymentStatus.Offen => "bg-secondary",
                OneTimePaymentStatus.TeilweiseInkassiert => "bg-warning",
                OneTimePaymentStatus.VollstaendigInkassiert => "bg-success",
                OneTimePaymentStatus.Verschoben => "bg-danger",
                OneTimePaymentStatus.Ueberzahlt => "bg-info",
                _ => "bg-danger"
            };

            return isBaseClass ? $"{baseClass} {colorClass} {textColor}" : $"{colorClass} {textColor}";
        }
        public bool IsEdited(OneTimePayment original, OneTimePayment edited)
        {
            return original.CustomerId != edited.CustomerId ||
                   original.DistributionLineId != edited.DistributionLineId ||
                   original.TotalAmount != edited.TotalAmount ||
                   original.AmountCollected != edited.AmountCollected ||
                   original.Status != edited.Status ||
                   original.Notes != edited.Notes;
        }

        public class CachedLine
        {
            public int LineId { get; set; }
            public bool NeedServerRefresh { get; set; } = true;
            public List<OneTimePaymentsGroupDto> Groups { get; set; } = [];
        }
    }
}
