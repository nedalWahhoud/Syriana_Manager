using Syriana_Manager.Components.ProductsF;
using Syriana_Manager.Components.Model;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web;
namespace Syriana_Manager.Components.Share
{
    public class WhatsAppService(IJSRuntime JS, ProductService productService)
    {
        private readonly IJSRuntime _JS = JS;
        private readonly ProductService _productService = productService;

        public async Task<ValidationResult> SendCustomerInfo(Customers customer)
        {
            try
            {
                if (customer == null)
                {
                    return new ValidationResult { Result = false, Message = "Kundendaten sind null." };
                }

                string message = GetMessage(customer);

                await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsAppWithoutNumber", message);

                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> SendCustomerInfo(List<Customers> customers)
        {
            try
            {
                if (customers == null || customers.Count == 0)
                {
                    return new ValidationResult { Result = false, Message = "Keine Kundendaten vorhanden." };
                }

                // alle customers in eine Nachricht zusammenfassen
                var messageBuilder = new StringBuilder();
                foreach (var customer in customers)
                {
                    messageBuilder.AppendLine(GetMessage(customer));
                    messageBuilder.AppendLine("--------------------------------------------------");
                }

                string message = messageBuilder.ToString();

                //
                await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsAppWithoutNumber", message);

                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        private static string GetMessage(Customers customer)
        {
            string mapsLink;
            if (customer.Latitude != 0 && customer.Longitude != 0)
            {
                mapsLink = $"https://maps.google.com/?q={customer.Latitude.ToString(CultureInfo.InvariantCulture)},{customer.Longitude.ToString(CultureInfo.InvariantCulture)}";
            }
            else
            {
                string addressQuery = $"{customer.Street} {customer.BuildingNumber}, {customer.PostalCode} {customer.City}";
                // endcode the mapsLink
                string encodedAddress = WebUtility.UrlEncode(addressQuery);

                mapsLink = $"https://maps.google.com/?q={encodedAddress}";
            }

            string stopNumber = $"🛑 Stop-Nummer" +
                                $": {customer.StopNumber}\n";

            string distributionLineInfo = null!;
            if (customer.DistributionLine != null)
            {
                distributionLineInfo = $"🚚 Richtung: " +
                                       $"{customer.DistributionLine.LineName}\n";
            }



            string name = $"👤 Kunde:{customer.Name_de} " +
                          $": {customer.Name_ar}\n";

            string Address = null!;
            if (!string.IsNullOrEmpty(customer.Street) &&
               !string.IsNullOrEmpty(customer.BuildingNumber) &&
               !string.IsNullOrEmpty(customer.PostalCode) &&
               !string.IsNullOrEmpty(customer.City))
            {
                Address = $"📍 Adresse: {customer.Street} {customer.BuildingNumber}, " +
                          $"{customer.PostalCode} {customer.City}\n";
            }

            string location = $"🗺️ Standort: " +
                              $"{mapsLink}\n";

            string phone = null!;
            if (!string.IsNullOrEmpty(customer.PhoneNumber))
                phone = $"📞 Tel.: {customer.PhoneNumber}\n";

            string email = null!;
            if (!string.IsNullOrEmpty(customer.Email))
                email = $"📧 E-Mail: {customer.Email}\n";

            string notes_de = null!;
            if (!string.IsNullOrEmpty(customer.Notes_de))
                notes_de = $"📝 Note (DE): {customer.Notes_de}\n";
            string notes_ar = null!;
            if (!string.IsNullOrEmpty(customer.Notes_ar))
                notes_ar = $"📝 Note (AR): {customer.Notes_ar}";

            string message =
                             $"{stopNumber}" +
                             $"{distributionLineInfo}" +
                             $"{name}" +
                             $"{Address}" +
                             $"{location}" +
                             $"{phone}" +
                             $"{email}" +
                             $"{notes_de}" +
                             $"{notes_ar}";

            return message;
        }
        public async Task<ValidationResult> SendTransactionCustomerNotify(Customers customer, DebtCustomers debtCustomers, TransactionsCustomers transactionsCustomers, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                    return new ValidationResult { Result = false, Message = "Token ist erforderlich." };

                // last transaction konfigurieren
                string transactionType = transactionsCustomers?.Type == TransactionType.Repay ? "zurückgezahlt" : "ausgeliehen";
                string lastTransactionInfo = string.Empty;
                if (transactionsCustomers != null)
                    lastTransactionInfo = $"💵 Letzter Transaktionsbetrag: {transactionsCustomers?.Amount ?? 0} €  {transactionType} \n";

                // url encode the token 
                string encodedToken = HttpUtility.UrlEncode(token);
                string baseUrl = $"{AppConfig.Domin}/customerDashboard";
                string urlWithToken = $"{baseUrl}?token={encodedToken}";


                string message =
                  $"Hallo {customer.Name_de} 👋 \n"
                + $"🔔 Benachrichtigung über Ihren aktuellen Kontostand.\n"
                + $"💰 Dein Schuldenstand: {debtCustomers?.Balance ?? 0} €\n"
                + lastTransactionInfo
                + "Hier klicken, um die Details zu sehen:\n"
                + $"{urlWithToken}";

                if (!string.IsNullOrEmpty(customer.PhoneNumber))
                    await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsApp", customer.PhoneNumber, message);
                else
                    await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsAppWithoutNumber", message);
                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> SendSupplierOrder(Suppliers supplier, Dictionary<int, int> selectedQuantities, string selectedLanguage)
        {
            try
            {
                if (supplier == null)
                    return new ValidationResult { Result = false, Message = "Lieferantendaten sind null." };
                if (selectedQuantities == null || selectedQuantities.Count == 0)
                    return new ValidationResult { Result = false, Message = "Keine ausgewählten Artikel für die Bestellung." };
                var sb = new StringBuilder();
                string rlm = "\u200F"; // Rechts-nach-links-Textrichtungszeichen
                string nbsp = "\u00A0"; // Ein Leerzeichen, das Zeilenumbrüche verhindert

                if (selectedLanguage == "ar")
                {
                    sb.AppendLine($"مرحباً {supplier.Name} 👋");
                    sb.AppendLine("هذه هي طلبيتي:");
                    sb.AppendLine($"{rlm}--------------------------");
                }
                else
                {
                    sb.AppendLine($"Hallo {supplier.Name} 👋");
                    sb.AppendLine("Hier ist meine Bestellung:");
                    sb.AppendLine($"--------------------------");
                }

                int index = 0;


                foreach (var item in selectedQuantities)
                {
                    index++;

                    Products product = _productService.GetProductByIdLocal(item.Key) ?? await _productService.GetProductByIdAsync(item.Key);
                    if (product == null)
                    {
                        sb = new();
                        break;
                    }

                    if (selectedLanguage == "ar")
                    {
                        sb.AppendLine($"{nbsp}{rlm}{index.ToString()}-{product.Name_ar} | الكمية: {item.Value}");
                    }
                    else
                    {
                        sb.AppendLine($"{index}- {product.Name_de}, Menge: {item.Value}");
                    }
                }
                if (sb.Length > 0)
                {
                    //  Footer
                    if (selectedLanguage == "ar")
                    {
                        sb.AppendLine($"{rlm}--------------------------");
                        sb.AppendLine($"{rlm}شكراً لك!");
                    }
                    else
                    {
                        sb.AppendLine("--------------------------");
                        sb.AppendLine("Vielen Dank!");
                    }
                    await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsApp", supplier.Phone, sb.ToString());
                    return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
                }
                else
                    return new ValidationResult { Result = false, Message = "Ein oder mehrere Produkte konnten nicht gefunden werden." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }

        }
        public async Task<ValidationResult> SendMassage(Customers customer,string? message = null)
        {
            try
            {
                if (customer == null)
                    return new ValidationResult { Result = false, Message = "Kundendaten sind null." };
                if (string.IsNullOrEmpty(customer.PhoneNumber))
                    return new ValidationResult { Result = false, Message = "Keine Telefonnummer für diesen Kunden vorhanden." };

                await _JS.InvokeVoidAsync("whatsappRedirect.openWhatsApp", customer.PhoneNumber, message);
                return new ValidationResult { Result = true, Message = "WhatsApp-Nachricht wurde erfolgreich geöffnet." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
    }
}
