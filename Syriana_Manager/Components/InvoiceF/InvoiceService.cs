using iTextSharp.text;
using iTextSharp.text.pdf;
using Syriana_Manager.Components.OrderF;
using Microsoft.Extensions.Options;
using Syriana_Manager.Components.Model;

namespace Syriana_Manager.Components.InvoiceF
{
    public class InvoiceService(HttpClient http, OrderService orderService)
    {
        private readonly HttpClient _http = http;
        private readonly OrderService _orderService = orderService;

        public List<BankTransferDetails> DownloadedBankTransferDetails = [];
        public async Task<ValidationResult> GetBankTransferDetailsAsync()
        {
            if (DownloadedBankTransferDetails.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Bank Transfer Details bereits geladen." };
            }

            try
            {
                var response = await _http.GetAsync("api/Invoices/getBankTransferDetails");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler" };
                }

                var bankTransferDetails = await response.Content.ReadFromJsonAsync<List<BankTransferDetails>>() ?? null;
                if (bankTransferDetails == null)
                {
                    return null!;
                }

                DownloadedBankTransferDetails.AddRange(bankTransferDetails);
                return new ValidationResult { Result = true, Message = "Bank Transfer Details erfolgreich geladen." };

            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<byte[]> InvoicePdfGeneration(Invoice invoice, PriceSummary priceSummary, TaxDetail Tax7, TaxDetail Tax19, DiscountDetails discountDetails)
        {
            var culture = new System.Globalization.CultureInfo("de-DE");

            if (DownloadedBankTransferDetails.Count == 0)
            {
                await GetBankTransferDetailsAsync();
            }
            // get projectinfo

            try
            {
                using var memoryStream = new MemoryStream();
                // Dokumenteinstellungen, Seitengröße und Ränder
                Document document = new(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                writer.PageEvent = new PdfEvent(DownloadedBankTransferDetails);

                document.Open();

                // space table
                PdfPTable spaceTable = new(1)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    LockedWidth = true,
                };
                Paragraph spaceP2 = new("  ", FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
                var spaceCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                spaceCell.AddElement(spaceP2);
                spaceTable.AddCell(spaceCell);
                spaceTable.SpacingAfter = 35f;
                document.Add(spaceTable);

                // address 
                PdfPTable addressTable = new(2)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    LockedWidth = true,
                };

                PdfPCell addressCell = new()
                {
                    Border = Rectangle.NO_BORDER
                };

                // absender
                string senderAddress = $"{ProjectInfo.Name} {ProjectInfo.Address.Replace("\n", "")}";
                Font font = FontFactory.GetFont(FontFactory.HELVETICA, 6, BaseColor.BLACK);
                Chunk underlinedSender = new(senderAddress, font);
                underlinedSender.SetUnderline(0.1f, -1f);
                Paragraph absenderP = new()
                {
                    SpacingAfter = 1f,
                };
                absenderP.Add(underlinedSender);
                addressCell.AddElement(absenderP);
                // empfanger
                if (invoice.CurrentOrder.Address != null)
                {
                    string recipientAddressName = $"{invoice.CurrentOrder.Address.FirstName} {invoice.CurrentOrder.Address.LastName}";
                    Paragraph p1 = new(recipientAddressName, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                    {
                        SpacingAfter = 1f // Leerzeichen nach dem Absatz
                    };
                    addressCell.AddElement(p1);
                    string recipientAddress = $"{invoice.CurrentOrder.Address!.Street}";
                    Paragraph p2 = new(recipientAddress, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                    {
                        SpacingAfter = 1f,

                    };
                    addressCell.AddElement(p2);
                    string zipCodeCity = $"{invoice.CurrentOrder.Address.ZipCode} {invoice.CurrentOrder.Address.City}";
                    Paragraph p3 = new(zipCodeCity, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
                    addressCell.AddElement(p3);
                }

                // add cell to table
                addressTable.AddCell(addressCell);

                // contact
                var contectCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                string website = $"Webseite: {ProjectInfo.Website}";
                Paragraph ContactP1 = new(website, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                {
                    SpacingAfter = 1f // Leerzeichen nach dem Absatz
                };
                contectCell.AddElement(ContactP1);
                string Email = $"E-Mail: {ProjectInfo.Email}";
                Paragraph ContactP2 = new(Email, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                {
                    SpacingAfter = 1f // Leerzeichen nach dem Absatz
                };
                contectCell.AddElement(ContactP2);
                string Tel = $"Tel.: {ProjectInfo.Phone}";
                Paragraph ContactP3 = new(Tel, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK));
                contectCell.AddElement(ContactP3);


                // add cell to table
                addressTable.AddCell(contectCell);
                // add to document
                addressTable.SpacingAfter = 40f;
                document.Add(addressTable);


                // Invoice Info
                PdfPTable InvoiceInfoTable = new(4)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                    LockedWidth = true,
                };
                // rechnung text
                string Invoice = "Rechnung";
                Paragraph InvoiceP = new(Invoice, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK))
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingAfter = 10f
                };

                var InvoiceTextCell = new PdfPCell(InvoiceP)
                {
                    Border = Rectangle.NO_BORDER,
                    Colspan = 4,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                InvoiceInfoTable.AddCell(InvoiceTextCell);

                // Rechnung info
                var InvoiceInfoCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                Font font1 = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                // Erstelle die vier Zellen in einer einzigen Zeile
                PdfPCell cell1 = new(new Phrase($"Rechnungsnummer\n{invoice.InvoceeNumber}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                PdfPCell cell2 = new(new Phrase($"Rechnungsdatum\n{DateTime.Today:dd.MM.yyyy}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                PdfPCell cell3 = new(new Phrase($"Ordersdatum\n{invoice.CurrentOrder.OrderDate:dd.MM.yyyy HH:mm}", font1))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 5f
                };

                // add to cell
                InvoiceInfoTable.AddCell(cell1);
                InvoiceInfoTable.AddCell(cell2);
                InvoiceInfoTable.AddCell(cell3);
                // add ti table
                InvoiceInfoTable.AddCell(InvoiceInfoCell);
                document.Add(InvoiceInfoTable);

                // order items
                PdfPTable orderItemsTable = new(6)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 20f
                };
                // set widths to table
                orderItemsTable.SetWidths([1f, 4f, 1f, 2f, 1f, 2f]);
                // font
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                // header column
                string[] headers = ["Pos.", "Produkt", "Menge", "Einzelpreis", "Steuer", "Preis"];
                foreach (string header in headers)
                {
                    PdfPCell headerCell = new(new Phrase(header, headerFont))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    orderItemsTable.AddCell(headerCell);
                }
                // add order items 
                for (int i = 0; i < invoice.CurrentOrder.OrderItems.Count; i++)
                {
                    var item = invoice.CurrentOrder.OrderItems[i];
                    var itemPreis = item.UnitPrice * item.Quantity;

                    orderItemsTable.AddCell(new PdfPCell(new Phrase((i + 1).ToString(), cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product?.Name_de ?? "Fehler", cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.UnitPrice.ToString("C", culture), cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(item.Product?.TaxRate?.Rate.ToString() != null ? item.Product.TaxRate.Rate.ToString() : "", cellFont)) { Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(itemPreis.ToString("C", culture), cellFont)) { Padding = 5 });
                }
                // Preis vor Rabbat 
                if (invoice.CurrentOrder.DiscountCode != null || invoice.CurrentOrder.DiscountCategory != null)
                {
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("Preis vor Rabatt", cellFont)) { Colspan = 5, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(discountDetails.PriceBeforeDiscount.ToString("C", culture), cellFont)) { Padding = 5 });
                }
                if (invoice.CurrentOrder.DiscountCode != null)
                {
                    orderItemsTable.AddCell(new PdfPCell(new Phrase($"Rabbat-{discountDetails.CategoryName}", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase($"{discountDetails.DiscountPercentage}{(discountDetails.DiscountType == DiscountType.Percentage ? "%":"€")}" +
                        $"({discountDetails.DiscountValue.ToString("C", culture)})", cellFont)) { Colspan = 4, Padding = 5 });
                }
                else if (invoice.CurrentOrder.DiscountCategory != null)
                {
                    // Rabattwertzeile
                    orderItemsTable.AddCell(new PdfPCell(new Phrase($"Rabbat-{discountDetails.CategoryName}", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase($"{discountDetails.DiscountPercentage} {(discountDetails.DiscountType == DiscountType.Percentage ? "%" : "€")} " +
                        $"({discountDetails.DiscountValue.ToString("C", culture)})", cellFont)) { Colspan = 4, Padding = 5 });

                }
                else
                {
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("Rabbat", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("0 €", cellFont)) { Colspan = 4, Padding = 5 });
                }
                // Versandkosten
                if (invoice.CurrentOrder != null && invoice.CurrentOrder.ShippingProviderId != null && invoice.CurrentOrder.ShippingProviders == null)
                {
                    await _orderService.GetShippingProvidersAsync();
                    invoice.CurrentOrder.ShippingProviders = _orderService.GetShippingProviderByIdLocal(invoice.CurrentOrder.ShippingProviderId ?? 0);
                }

                if (invoice.CurrentOrder!.DiscountCode != null || invoice.CurrentOrder.DiscountCategory != null)
                {
                    orderItemsTable.AddCell(new PdfPCell(new Phrase("Preis nach Rabatt", cellFont)) { Colspan = 5, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                    orderItemsTable.AddCell(new PdfPCell(new Phrase(discountDetails.PriceAfterDiscount.ToString("C", culture), cellFont)) { Padding = 5 });
                }
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Versandskosten", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(priceSummary.ShippingCost.ToString("C", culture), cellFont)) { Colspan = 4, Padding = 5 });
                // Gesamtbrutto
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Gesamtbrutto", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(priceSummary.TotalGross.ToString("C", culture), cellFont)) { Colspan = 4, Padding = 5 });
                // Steuer 
                // 19%
                orderItemsTable.AddCell(new PdfPCell(new Phrase($"MwSt. 19% von {Tax19.BaseAmount.ToString("C", culture) + (Tax19.ShippingPart > 0 ? "+" + Tax19.ShippingPart.ToString("C", culture) : string.Empty)}", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase((Tax19.TaxAmount + Tax19.TaxShippingAmount).ToString("C", culture), cellFont)) { Colspan = 4, Padding = 5 });
                // 7%
                orderItemsTable.AddCell(new PdfPCell(new Phrase($"MwSt. 7% von {Tax7.BaseAmount.ToString("C", culture) + (Tax7.ShippingPart > 0 ? "+" + Tax7.ShippingPart.ToString("C", culture) : string.Empty)}", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase((Tax7.TaxAmount + Tax7.TaxShippingAmount).ToString("C", culture), cellFont)) { Colspan = 4, Padding = 5 });
                // Netto
                orderItemsTable.AddCell(new PdfPCell(new Phrase("Gesamtnetto", cellFont)) { Colspan = 5, Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                orderItemsTable.AddCell(new PdfPCell(new Phrase(priceSummary.TotalNet.ToString("C", culture), cellFont)) { Colspan = 4, Padding = 5 });
                document.Add(orderItemsTable);

                document.Close();

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler bei der PDF-Erstellung: " + ex.Message);
            }
        }
        public (string Code, int DiscountPercentage, double DiscountValue, DiscountType DiscountType) GetDetailsDiscountCode(Order order)
        {

            (string Code,int DiscountPercentage, double DiscountValue, DiscountType DiscountType) discountDetails = default!;
            double originalTotal = order.OrderItems.Sum(x => x.UnitPrice * x.Quantity);
            double discountAmt = order.DiscountCode?. DiscountAmount ?? 0;

            double discountValue = originalTotal - order.TotalPrice;

            discountValue = Math.Clamp(discountValue, 0, originalTotal);

            discountDetails.Code = order.DiscountCode?.Code ?? string.Empty;
            discountDetails.DiscountPercentage = order.DiscountCode?.DiscountAmount ?? 0;
            discountDetails.DiscountValue = discountValue;
            discountDetails.DiscountType = order.DiscountCode?.DiscountType ?? DiscountType.None;
            return discountDetails;
        }
        public (string Code, int DiscountPercentage, double DiscountValue, string categoryName, DiscountType DiscountType) GetDetailsDiscountCategory(Order order)
        {
            (string Code,int DiscountPercentage, double DiscountValue, string categoryName, DiscountType DiscountType) discountDetails = default!;

            // get discount details for the order by category
            double categoryitemsPrice = 0;
            foreach (var item in order.OrderItems)
            {
                if (item.CategoryId == (order.DiscountCategory?.CategoriesId ?? 0))
                {
                    categoryitemsPrice += item.UnitPrice * item.Quantity;
                }
            }
            // get discount category
            double categoryDiscountValue = 0;

            if(order.DiscountCategory!.DiscountType == DiscountType.Percentage)
                categoryDiscountValue = Math.Min(categoryitemsPrice * (order.DiscountCategory?.DiscountAmount ?? 0) / 100.0, categoryitemsPrice);
            else
                categoryDiscountValue = Math.Min((double)(order.DiscountCategory?.DiscountAmount ?? 0), categoryitemsPrice);

            discountDetails.Code = order.DiscountCategory?.Code ?? string.Empty;
            discountDetails.DiscountPercentage = order.DiscountCategory?.DiscountAmount ?? 0;
            discountDetails.DiscountValue = categoryDiscountValue;

            // get category name
            var matchedItem = order.OrderItems
            .FirstOrDefault(o => o.CategoryId == order.DiscountCategory?.CategoriesId);
            discountDetails.categoryName = matchedItem?.Product?.Category?.Name_de ?? "Kein Kategorie\"";
            discountDetails.DiscountType = order.DiscountCategory?.DiscountType ?? DiscountType.None;
            return discountDetails;
        }

        public double GetTotalPriceBeforeDiscount(Order order)
        {
            if (order.DiscountCode != null)
            {
                if (order.DiscountCode.DiscountType == DiscountType.FixedAmount)
                    return Math.Min(order.TotalPrice + (order.DiscountCode?.DiscountAmount ?? 0), order.TotalPrice);
                else
                    {
                    double discountPercent = order.DiscountCode?.DiscountAmount ?? 0;

                    double originalTotal = (discountPercent >= 100)
                        ? order.TotalPrice
                        : order.TotalPrice / (1 - (discountPercent / 100.0));

                    return originalTotal;
                }
            }
            else if (order.DiscountCategory != null)
            {
                double categoryitemsPrice = 0;
                foreach (var item in order.OrderItems)
                {
                    if (item.CategoryId == (order.DiscountCategory?.CategoriesId ?? 0))
                    {
                        categoryitemsPrice += item.UnitPrice * item.Quantity;
                    }
                }
                double discountValue;
                if (order.DiscountCategory!.DiscountType == DiscountType.FixedAmount)
                    discountValue = Math.Min((double)order.DiscountCategory.DiscountAmount, categoryitemsPrice);
                else
                    discountValue = Math.Min(categoryitemsPrice * (order.DiscountCategory?.DiscountAmount ?? 0) / 100.0, categoryitemsPrice);

                return order.TotalPrice + discountValue;

            }
            return order.TotalPrice;
        }
        public double GetTotalPriceAfterDiscount(Order order)
        {
            return order.TotalPrice;
        }

        public double GetTotalPriceByTaxRate(Order order, double taxRate)
        {
            double totalPrice = 0;

            // um die steuer von Versand zu rechnen
            int productsTaxRateCount = 0;

            foreach (var item in order.OrderItems)
            {
                double itemPreis = item.UnitPrice * item.Quantity;

                if (item.Product?.TaxRate != null && item.Product.TaxRate.Rate == taxRate)
                {
                    if (order.DiscountCode != null)
                    {
                        if (order.DiscountCode.DiscountType == DiscountType.Percentage)
                        {
                            double discountValue = (itemPreis / 100) * order.DiscountCode.DiscountAmount;
                            double priceAfterDiscount = itemPreis - discountValue;
                            totalPrice += Math.Max(0, priceAfterDiscount);
                        }
                        else
                        {
                            double priceAfterDiscount = itemPreis - order.DiscountCode.DiscountAmount;

                            totalPrice += Math.Max(0, priceAfterDiscount);
                        }
                    }
                    else if (order.DiscountCategory != null && order.DiscountCategory.CategoriesId == item.CategoryId)
                    {
                        if (order.DiscountCategory.DiscountType == DiscountType.Percentage)
                        {
                            double discountValue = (itemPreis / 100) * order.DiscountCategory.DiscountAmount;
                            double priceAfterDiscount = itemPreis - discountValue;
                            totalPrice += Math.Max(0, priceAfterDiscount);
                        }
                        else
                        {
                            double priceAfterDiscount = itemPreis - order.DiscountCategory.DiscountAmount;

                            totalPrice += Math.Max(0, priceAfterDiscount);
                        }
                    }
                    else
                    {
                        totalPrice += itemPreis;
                    }
                    productsTaxRateCount++;
                }
            }

            return totalPrice;
        }
        public (double shippingPart7, double shippingPart19) GetShippingTax(double ShippingCost, double net7, double net19)
        {
            double totalNetItems = net7 + net19;
            if (totalNetItems <= 0)
                return (0, 0);

            double shippingPart7 = ShippingCost * (net7 / totalNetItems);

            double shippingPart19 = ShippingCost - shippingPart7;

            return (shippingPart7, shippingPart19);
        }
    }

    public class PdfEvent(List<BankTransferDetails> bankDetails) : PdfPageEventHelper
    {

        private readonly List<BankTransferDetails> _bankDetails = bankDetails;

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            PdfPTable headerTable = new(1)
            {
                TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin
            };
            PdfPCell cell;
            try
            {


                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string logoUrlFromConfig = ProjectInfo.LogoUrl.TrimStart('/');
                string fullPath = Path.Combine(wwwrootPath, logoUrlFromConfig);
                Image logo = Image.GetInstance(fullPath);

                // Verstellen die Größe
                logo.ScaleToFit(110f, 110f);

                cell = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingLeft = 3
                };
            }
            catch (Exception)
            {
                // Falls das Bild nicht geladen werden kann, wird alternativ der Name in Fettdruck angezeigt (Fallback).
                cell = new PdfPCell(new Phrase(ProjectInfo.Name, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 25, BaseColor.BLACK)))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingBottom = 10
                };
            }
            headerTable.AddCell(cell);
            // Wir zeichnen die Tabelle oben auf die Seite (links und in einem gewissen Abstand vom oberen Seitenrand).
            headerTable.WriteSelectedRows(0, -5, document.LeftMargin - 20, document.PageSize.Height - 10, writer.DirectContent);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            for (int i = 0; i < _bankDetails.Count; i++)
            {
                var detail = _bankDetails[i]; // مثال: عرض أول عنصر فقط

                string BankDetails = $"Bankverbindung \n" +
                    $"{detail.AccountHolderName}\n " +
                    $"{detail.BankName}\n" +
                    $"{detail.IBAN}\n" +
                    $"{detail.BIC}";

                string projectInfo1 = $"{ProjectInfo.Name}\n" +
                    $"{ProjectInfo.Address}\n" +
                    $"Steuernummer: {ProjectInfo.Steuernummer}\n" +
                    $"USt-IdNr.:{ProjectInfo.UStIdNr}";

                PdfPTable footerTable = new(2)
                {
                    TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin,
                };

                footerTable.SetWidths([1f, 1f]); // 50% - 50% توزيع الأعمدة

                var leftCell = new PdfPCell(new Phrase(projectInfo1, FontFactory.GetFont(FontFactory.HELVETICA, 8)))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderWidthTop = 0.5f,
                    BorderColorTop = BaseColor.GRAY,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_TOP,
                };

                var rightCell = new PdfPCell(new Phrase(BankDetails, FontFactory.GetFont(FontFactory.HELVETICA, 8)))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderWidthTop = 0.5f,
                    BorderColorTop = BaseColor.GRAY,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_TOP,
                };

                footerTable.AddCell(leftCell);
                footerTable.AddCell(rightCell);

                // موقع الجدول في أسفل الصفحة: من اليسار LeftMargin، وعلى ارتفاع BottomMargin
                float footerY = document.BottomMargin;
                footerTable.WriteSelectedRows(0, -1, document.LeftMargin, footerY, writer.DirectContent);
            }
        }
    }

    public class TaxDetail
    {
        public double Rate { get; set; }
        public double BaseAmount { get; set; }
        public double TaxAmount => Math.Round(BaseAmount * (Rate / 100), 2);
        public double ShippingPart { get; set; }
        public double TaxShippingAmount => Math.Round(ShippingPart * (Rate / 100), 2);
        public double NetAmount => BaseAmount - TaxAmount;
    }
    public class PriceSummary
    {
        public double ShippingCost { get; set; }
        public double TotalPrice { get; set; }
        public double TotalGross { get; set; }
        public double TotalNet { get; set; }
    }
    public class DiscountDetails
    {
        public string CategoryName { get; set; } = string.Empty;
        public int DiscountPercentage { get; set; }
        public double DiscountValue { get; set; }
        public double PriceBeforeDiscount { get; set; }
        public double  PriceAfterDiscount { get; set; }
        public DiscountType DiscountType { get; set; }
    }
}
