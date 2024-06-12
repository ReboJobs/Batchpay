using Core;
using IronPdf;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence.Config;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using System;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Model.Identity;
using Xero.NetStandard.OAuth2.Models;
using XeroApp.Models.BusinessModels;
using XeroApp.Services;
using XeroApp.Services.ApiClientService;
using XeroApp.Services.UserTrackService;
using XeroApp.Services.OrganisationService;
using XeroApp.Services.SubscriptionService;
using XeroApp.Services.ApiLogsService;
using XeroApp.Utilities;
using System.Net;
using XeroApp.Extensions;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Microsoft.Data.SqlClient;
using FileHelpers;
using XeroApp.Models.BusinessModels.Bacs;
using XeroApp.Filters;
using XeroApp.Services.ReportBugService;

namespace XeroApp.Controllers
{
    public class XeroController : Controller
    {
        private readonly XeroAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IExportService _exportSvc;
        private readonly IUserService _userSvc;
        private readonly IXeroService _xeroSvc;
        private readonly IEmailService _emailSvc;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IApiClientService _apiClientSvc;
        private readonly IUserTrackService _userSvcTrack;
        private UserTrackModel userTrack = new UserTrackModel();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISession _session;
        private readonly IOrganisationService _organisationService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IReportBugService _reportBugService;
        private readonly IApiLogsService _apiLogsService;

        public XeroController(
            XeroAppDbContext db,
            IConfiguration config,
            IExportService exportSvc,
            IUserService userSvc,
            IUserTrackService userSvcTrack,
            IXeroService xeroSvc,
            IOptions<XeroConfiguration> XeroConfig,
            IEmailService emailSvc,
            IHttpContextAccessor httpContextAccessor,
            IOrganisationService organisationService,
            ISubscriptionService subscriptionService,
            IApiClientService apiClientSvc,
            IReportBugService reportBugService,
            IApiLogsService apiLogsService)

        {
            _db = db;
            _config = config;
            _exportSvc = exportSvc;
            _userSvc = userSvc;
            _userSvcTrack = userSvcTrack;
            _xeroSvc = xeroSvc;
            _emailSvc = emailSvc ?? throw new ArgumentNullException(nameof(emailSvc));
            _apiClientSvc = apiClientSvc;
            _httpContextAccessor = httpContextAccessor;
            _organisationService = organisationService;
            _subscriptionService = subscriptionService;
            _session = _httpContextAccessor.HttpContext.Session;
            _reportBugService = reportBugService;
            _apiLogsService = apiLogsService;
            this.XeroConfig = XeroConfig;

        }

        [TenantTimeout]
        public async Task<IActionResult> Index()
        {
            try
            {
                SessionUtility session = new SessionUtility(_httpContextAccessor);
                var emailAddress = session.getSession();
                ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

                if (!TokenUtilities.TokenExists(emailAddress) || emailAddress == null)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return View();
        }

        [JsonExceptionFilter]
        [HttpGet]
        public async Task<JsonResult> GetClientApps()
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var clientAppDetails = await _db.ClientApps.Where(c => c.UserName == emailAddress).ToListAsync();
            return Json(clientAppDetails);
        }



        [HttpPost]
        public async Task<IActionResult> SaveXeroAppInfo([FromBody] XeroClientAppModel obj)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Enter required fields");
                }

                var clientExists = await _xeroSvc.ClientIdExistAsync(User.Identity?.Name, obj.ClientId);

                if (!clientExists)
                {
                    obj.UserName = User.Identity?.Name;
                    await _xeroSvc.AddClientAppAsync(obj);

                    return RedirectToAction("Index", "Xero");
                }
                else
                {
                    return BadRequest("Client Id exists.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IActionResult> BatchPayments()
        {
            try
            {
                SessionUtility session = new SessionUtility(_httpContextAccessor);
                var emailAddress = session.getSession();
                var accountingApi = new AccountingApi();
                var accessToken = _xeroSvc.GetTokenAsync(emailAddress);
                var tenID = _xeroSvc.GetTenantIdAsync(accessToken, emailAddress);
                var client = new XeroClient(XeroConfig.Value);
                var ListOfSelectedTenants = await client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress));

                _xeroSvc.InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                                       Global.globallistTenantLog[0].TenantName,
                                       DateTime.Now,
                                       Global.globallistTenantLog[0].TenantType,
                                       "200", "Get", client.xeroConfiguration.XeroApiBaseUri + "/Connections");


                var organizationInfo = accountingApi.GetOrganisationsAsync(accessToken, tenID).Result._Organisations.FirstOrDefault();

                
                var orgCountryCode = organizationInfo.CountryCode.ToString();
                ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;
                var apiLogsModel = new ApiLogsModel();

                if (!TokenUtilities.TokenExists(emailAddress) || emailAddress == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
                var utcTimeNow = DateTime.UtcNow;

                if (utcTimeNow > xeroToken.ExpiresAtUtc)
                {
                    var clientExpiry = new XeroClient(XeroConfig.Value);
                    xeroToken = (XeroOAuth2Token)clientExpiry.RefreshAccessTokenAsync(xeroToken).Result;
                    emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                    TokenUtilities.StoreToken(xeroToken, emailAddress);
                }

                var tenantId = TokenUtilities.GetCurrentTenantId(emailAddress);
                //var client = new XeroClient(XeroConfig.Value);
                //var ListOfSelectedTenants = await client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress));

                List<Tenant> tenants = new List<Tenant>();
                StringBuilder builderPromptTenantExpire = new StringBuilder();

                int ctr = 0;

                foreach (var item in ListOfSelectedTenants)
                {
                    var xeroTenantSubscription = _subscriptionService.SearchTenantSubscriptionAsync(item.TenantId.ToString()).Result;


                    if (DateTime.Now > xeroTenantSubscription.DateStart && DateTime.Now <= xeroTenantSubscription.DateEnd)
                    {
                        tenants.Add(item);
                    }
                    else
                    {

                        if (ctr == 0)
                        {
                            builderPromptTenantExpire.Append(item.TenantName);
                        }
                        else
                        {
                            builderPromptTenantExpire.Append(",");
                            builderPromptTenantExpire.Append(item.TenantName);
                        }
                    }
                    ctr = ctr + 1;
                }

                var test = ListOfSelectedTenants.Join(xeroToken.Tenants, t => t.TenantId, st => st.TenantId, (t, st) => new TenantDetails
                {
                    TenantName = t.TenantName,
                    TenantId = t.TenantId
                }).ToList();

                ViewBag.OrgPickerTenantList = tenants.Join(xeroToken.Tenants, t => t.TenantId, st => st.TenantId, (t, st) => new TenantDetails
                {
                    TenantName = t.TenantName,
                    TenantId = t.TenantId
                }).ToList();

                ViewBag.actionToRedirect = "BatchPayments";
                ViewBag.controllerToRedirect = "Xero";
                ViewBag.PromptPopUP = builderPromptTenantExpire.ToString();

                ViewBag.OrgPickerCurrentTenantId = tenantId;
                ViewBag.Statuses = new string[] { "AUTHORISED", "SUBMITTED", "DRAFT" };
                ViewBag.PageSizes = new string[] { "20", "50", "100", "500", "1000" };
                ViewBag.DefaultPageSize = 500;

                int days = (int)DateTime.Now.DayOfWeek;
                DateTime lastMonth = DateTime.Now.AddMonths(-1);
                ViewBag.weekStart = DateTime.Now.AddDays(-days);
                ViewBag.weekEnd = ViewBag.weekStart.AddDays(7 - days);
                ViewBag.monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                ViewBag.monthEnd = ViewBag.monthStart.AddMonths(1).AddDays(-1);
                ViewBag.lastMonthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                ViewBag.lastMonthEnd = ViewBag.lastMonthStart.AddMonths(1).AddDays(-1);
                ViewBag.lastYearStart = new DateTime(DateTime.Now.Year - 1, 1, 1);
                ViewBag.lastYearEnd = new DateTime(DateTime.Now.Year - 1, 12, 31);
                ViewBag.orgCountryCode = orgCountryCode;


                 _xeroSvc.InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                              Global.globallistTenantLog[0].TenantName,
                              DateTime.Now,
                              Global.globallistTenantLog[0].TenantType,
                              "200", "Get", accountingApi.Configuration.BasePath + "/Organisation");


                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public async Task<IActionResult> ExportToAbaFormatChecking([FromBody] ExportABAModel invoicePayment)
        {
            List<Contact> _contacts = new();
            var errorList = new List<ErrorIssueModel>();
            var countInvoices = invoicePayment.Invoices.Count;
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();

            if (invoicePayment.Invoices.Count > 0)
            {
                _contacts = await _xeroSvc.GetContactsAsync(emailAddress);
                foreach (var invoice in invoicePayment.Invoices)
                {
                    invoice.Contact = _contacts.Where(w => w.ContactID == invoice.Contact?.ContactID).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(invoice.Contact.BankAccountDetails) || invoice.Contact.BankAccountDetails == null)
                    {
                        var error = new ErrorIssueModel
                        {
                            Error = "No bankaccount details for " + invoice.InvoiceNumber
                        };
                        errorList.Add(error);
                    }
                }
            }

            if (errorList.Count > 0)
            {
                return Json(new { result = false, error = errorList, countInvoice = countInvoices, countIssue = errorList.Count });
            }
            else
            {
                return Json(new { result = true, error = "No Error", countInvoice = countInvoices, countIssue = 0 });
            }
        }



        [HttpPost]
        public async Task<IActionResult> ExportToBacsFormat([FromBody] ExportABAModel invoicePayment)
        {
            try
            {
                SessionUtility session = new SessionUtility(_httpContextAccessor);
                var emailAddress = session.getSession();
                ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

                if (!TokenUtilities.TokenExists(emailAddress) || emailAddress == null)
                    return RedirectToAction("Index", "Home");

                var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
                var utcTimeNow = DateTime.UtcNow;

                var engine = new MultiRecordEngine(typeof(VolRecord),
                                typeof(HDR1Record),
                                typeof(HDR2Record),
                                typeof(UHL1Record),
                                typeof(StandardRecords),
                                typeof(ContraRecord),
                                typeof(EOF1Record),
                                typeof(EOF2Record),
                                typeof(UTL1Record));

                var Contacts = _xeroSvc.GetContact(emailAddress).Result;

                var data = new List<object>();
                var volRecord = new VolRecord();

                var hdr1Record = new HDR1Record();
                hdr1Record.InitializedDependentFields(volRecord);

                var uhl1Record = new UHL1Record();
                uhl1Record.BacsProcessingDay = uhl1Record.BacsProcessingDayCondition(invoicePayment.TransactionDate);

                hdr1Record.CreationDate = uhl1Record.BacsProcessingDay;  //hdr1Record.CreationDateCondition(uhl1Record);
                hdr1Record.ExpirationDate = uhl1Record.BacsProcessingDay; //hdr1Record.ExpirationDateCondition(uhl1Record);

                var OriginatingSortCodeNumber = "402811";
                var OriginatingAccountNumber = invoicePayment.BankAccountNumber;

                var hdr2Record = new HDR2Record();
                var listOfStandardRecord = new List<StandardRecords>();

                var invoiceContact = invoicePayment.Invoices.Join(Contacts._Contacts,
                      i => i.Contact.ContactID,
                      c => c.ContactID,
                      (invoice, contact) => new
                      {
                          invoiceContactName = invoice.ContactName,
                          AmountDueEditable = invoice.AmountDueEditable,
                          contactID = contact.ContactID,
                          contactBankAccountName = (contact.BatchPayments is null) ? string.Empty : contact.BatchPayments.BankAccountName,
                          contactBankAccountNumber = (contact.BatchPayments is null) ? string.Empty : contact.BatchPayments.BankAccountNumber
                      }).GroupBy(ic => ic.contactID,
                      (key, allItemInTheGroup) => new
                      {
                          key,
                          amountDueTotalPerGroup = allItemInTheGroup.Sum(ic => ic.AmountDueEditable),
                          contactBankAccountName = allItemInTheGroup.Select(ic => ic.contactBankAccountName).FirstOrDefault(),
                          contactBankAccountNumber = allItemInTheGroup.Select(ic => ic.contactBankAccountNumber).FirstOrDefault(),
                      }).ToList();


                invoiceContact.ForEach(ic => listOfStandardRecord.Add(AddStadardRecordToList(invoicePayment.BankAccountName,
                    invoicePayment.BankAccountNumber,
                    ic.contactBankAccountName,
                    ic.contactBankAccountNumber,
                    ic.amountDueTotalPerGroup
                    )));

                var contraRecord = new ContraRecord();
                contraRecord.InitializedDependentFields(OriginatingSortCodeNumber,
                                                    OriginatingAccountNumber,
                                                    listOfStandardRecord,
                                                    String.Empty,
                                                    invoicePayment.BankAccountName);
                var eof1Record = new EOF1Record();
                eof1Record.InitializedDependentFields(hdr1Record);

                var eof2Record = new EOF2Record();
                eof2Record.InitializedDependentFields(hdr2Record);

                var utl1Record = new UTL1Record();

                data.Add(volRecord);
                data.Add(hdr1Record);
                data.Add(hdr2Record);
                data.Add(uhl1Record);
                listOfStandardRecord.ForEach(s => data.Add(s));
                data.Add(contraRecord);
                data.Add(eof1Record);
                data.Add(eof2Record);
                data.Add(utl1Record);

                var stream = new MemoryStream();
                TextWriter writer = new StreamWriter(stream) { AutoFlush = true };

                if (Convert.ToBoolean(Int32.Parse(invoicePayment.isinsertRecordDB)) == true)
                {
                    string JsonString = JsonSerializer.Serialize(invoicePayment);
                    _xeroSvc.SaveTransactionTrack(emailAddress, "ExportDataFormat", JsonString);
                }

                engine.WriteStream(writer, data);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "text/plain", $"file_{DateTime.Now.ToShortDateString().Replace("/", "")}.BAC");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private StandardRecords AddStadardRecordToList(string originBankAccountName,
            string oringBankAccountNumber,
            string contactAccountName,
            string contactAccountNumber,
            decimal? totalOfAmountDue)
        {

            return new StandardRecords
            {
                OriginatingSortCodeNumber = oringBankAccountNumber,
                OriginatingAccountNumber = oringBankAccountNumber,
                Amount = Decimal.ToInt32((decimal)totalOfAmountDue),
                Username = originBankAccountName,
                UserReference = "REF FOR BENE",
                DestinationAccountName = contactAccountName,
                DestinationAccountNumber = contactAccountNumber
            };
        }


        [HttpPost]
        public async Task<IActionResult> ExportToAbaFormat([FromBody] ExportABAModel invoicePayment)
        {
            try
            {

                StringBuilder strBuilder = new();
                List<Contact> _contacts = new();
                MemoryStream stream;
                decimal totalAmount = 0;
                decimal totalCreditAmount = 0;
                decimal totalDebitAmount = 0;
                int cntInvoiceWithBankAcctDet = 0;
                var errorList = new List<ErrorIssueModel>();
                SessionUtility session = new SessionUtility(_httpContextAccessor);
                var emailAddress = session.getSession();

                if (invoicePayment.Invoices.Count > 0)
                {
                    var email = User.XeroEmail();
                    _contacts = await _xeroSvc.GetContactsAsync(emailAddress);


                    var stringArray = _config.GetSection("FinancialCode3Digit").Get<string[]>().ToList();

                    var financialNum3Dig = invoicePayment.BankAccountNumber.Substring(0, 3);
                    var financialNumber = invoicePayment.BankAccountNumber.Substring(0, 2);
                    var financialInstCode = string.Empty;

                    if ((stringArray.Any(s => financialNum3Dig.Contains(s)))) //one of the items?
                    {
                        financialInstCode = await _xeroSvc.GetFinancialCodeAsync(financialNum3Dig);
                    }
                    else
                    {
                        financialInstCode = await _xeroSvc.GetFinancialCodeAsync(financialNumber);
                    }

                    string approvals = string.Empty;
                    string dailyPaymentApprovals = string.Empty;
                    string statements = invoicePayment.BankAccountName;
                    string security = "Payments";
                    string preference = string.Empty;

                    var statementsFieldDesc = statements.Length > 26 ? statements.Substring(0, 26).PadRight(16) : statements.PadRight(26);

                    var dateTime = DateTime.Parse(invoicePayment.TransactionDate, System.Globalization.CultureInfo.InvariantCulture);

                    // 0 - Descriptive Record
                    strBuilder.Append($"0" + approvals.PadRight(17) + $"01" + financialInstCode + dailyPaymentApprovals.PadRight(7) + statementsFieldDesc + $"000000" + security.PadRight(12) + $"{dateTime.ToString("ddMMyy").Replace("/", "")}" + preference.PadRight(40) + "\n");

                    /// 1 – Detail Record
                    foreach (var invoice in invoicePayment.Invoices)
                    {
                        invoice.Contact = _contacts.Where(w => w.ContactID == invoice.Contact?.ContactID).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(invoice.Contact.BankAccountDetails))
                        {
                            char[] acctDetails = invoice.Contact.BankAccountDetails.Replace("-", "").ToCharArray();
                            string BSB = "", AccountNumber = "";

                            for (int i = 0; i <= 5; i++)
                            {
                                if (i == 2)
                                {
                                    BSB += acctDetails[i] + "-";
                                }
                                else
                                {
                                    BSB += acctDetails[i];
                                }
                            }

                            int charLength = (acctDetails.Length - BSB.Length) + 1;

                            for (int i = 1; i <= charLength; i++)
                            {
                                AccountNumber += acctDetails[5 + i];
                            }

                            int? amount = (int?)(invoice.AmountDueEditable * 100);
                            var formattedAmt = String.Concat(Format(amount), amount?.ToString().Replace(",", "").Replace(".", "").Trim());
                            var concatValue = String.Concat("N50", formattedAmt.PadLeft(10));

                            var concatValueTrim = concatValue.Replace(" ", "0");

                            var payersBSB = invoicePayment.BankAccountNumber.Substring(0, 3) + '-' + invoicePayment.BankAccountNumber.Substring(3, 3);
                            var payersBankAccountNumber = invoicePayment.BankAccountNumber.Substring(6);


                            //implement substring if aba fields length exceeds the requirement
                            var BSBField = BSB.Length > 7 ? BSB.Substring(0, 7).PadRight(7) : BSB.PadRight(7);
                            var accountNumberField = AccountNumber.Length > 9 ? AccountNumber.Substring(0, 9).PadLeft(9) : AccountNumber.PadLeft(9);
                            var contactNameField = invoice.Contact.Name.Length > 32 ? invoice.Contact.Name.ToUpper().Substring(0, 32).PadRight(32) : invoice.Contact.Name.ToUpper().PadRight(32);
                            var invoiceNumberField = invoice.InvoiceNumber.Length > 18 ? invoice.InvoiceNumber.ToUpper().Substring(0, 18).PadRight(18) : invoice.InvoiceNumber.PadRight(18);
                            var payersBSBField = payersBSB.Length > 7 ? payersBSB.Substring(0, 7).PadRight(7) : payersBSB.PadRight(7);
                            var payersBankAccountNumberField = payersBankAccountNumber.Length > 9 ? payersBankAccountNumber.Substring(0, 9).PadLeft(9) : payersBankAccountNumber.PadLeft(9);
                            var statementsField = statements.Length > 16 ? statements.Substring(0, 16).PadRight(16) : statements.PadRight(16);

                            strBuilder.Append($"1" + BSBField + $"{accountNumberField}" + concatValueTrim + contactNameField + invoiceNumberField + payersBSBField + payersBankAccountNumberField + statementsField + $"0".PadRight(8, '0') + "\n");
                            cntInvoiceWithBankAcctDet++;

                        }


                        if (invoice.Contact.BankAccountDetails != null)
                        {

                            if (!string.IsNullOrWhiteSpace(invoice.Contact.BankAccountDetails))
                            {
                                totalAmount = totalAmount + ((invoice.AmountDue.HasValue ? invoice.AmountDue.Value : 0));
                                totalCreditAmount = totalCreditAmount + (invoice.AmountDue.HasValue ? invoice.AmountDue.Value : 0);
                                totalDebitAmount = 0;
                            }

                        }

                    }

                    string blankField = string.Empty;
                    string blankField2 = string.Empty;
                    string blankField3 = string.Empty;


                    string strtotalAmount = String.Format("{0:0.00}", totalAmount);
                    string strtotalCreditAmount = String.Format("{0:0.00}", totalCreditAmount);
                    string strtotalDebitAmount = String.Format("{0:0.00}", totalDebitAmount);

                    int pos = strtotalAmount.IndexOf(".");
                    int poscredit = strtotalCreditAmount.IndexOf(".");
                    int posdebit = strtotalDebitAmount.IndexOf(".");

                    if (pos >= 0)
                    {
                        strtotalAmount = strtotalAmount.Remove(pos, 1);
                    }

                    if (poscredit >= 0)
                    {
                        strtotalCreditAmount = strtotalCreditAmount.Remove(poscredit, 1);
                    }

                    if (posdebit >= 0)
                    {
                        strtotalDebitAmount = strtotalDebitAmount.Remove(posdebit, 1);
                    }

                    //7 – Batch Control Record
                    //strBuilder.Append($"7999-999" + blankField.PadRight(12) + strtotalAmount.PadLeft(10, '0') + strtotalCreditAmount.PadLeft(10, '0') + strtotalDebitAmount.PadLeft(10, '0') + blankField2.PadRight(24) + $"{ invoicePayment.Invoices.Count}".PadLeft(6, '0') + blankField3.PadRight(40));
                    strBuilder.Append($"7999-999" + blankField.PadRight(12) + strtotalAmount.PadLeft(10, '0') + strtotalCreditAmount.PadLeft(10, '0') + strtotalDebitAmount.PadLeft(10, '0') + blankField2.PadRight(24) + $"{cntInvoiceWithBankAcctDet}".PadLeft(6, '0') + blankField3.PadRight(40));

                }
                var data = _exportSvc.ExportToTextFile(strBuilder.ToString());
                stream = new MemoryStream(data);

                userTrack = await _apiClientSvc.GetUserIPAsync();
                userTrack.Browser = string.Empty;
                userTrack.Browser = Global.GlobalBrowser;//await JSRuntime.InvokeAsync<string>(identifier: "identifyBrowser");
                userTrack.DateTimeLog = DateTime.Now;
                userTrack.UserName = emailAddress;
                userTrack.userEmail = emailAddress;
                userTrack.Page = "Xero";
                userTrack.methodName = "ExportToABAFormat";
                userTrack.TotalNumInvoiceTransaction = cntInvoiceWithBankAcctDet;
                userTrack.TotalInvoiceTransactionErr = invoicePayment.errorCount;
                userTrack.totalInvoiceAmount = totalAmount;


                if (Convert.ToBoolean(Int32.Parse(invoicePayment.isinsertRecordDB)) == true)
                {
                    string JsonString = JsonSerializer.Serialize(invoicePayment);
                    _xeroSvc.SaveTransactionTrack(emailAddress, "ExportDataFormat", JsonString);
                }


                await _userSvcTrack.InsertUserTrackAsync(userTrack);
                return File(stream, "text/plain", $"file_{DateTime.Now.ToShortDateString().Replace("/", "")}.txt");
            }
            catch (Exception ex)
            {
                ReportBugModel reportBugModel = new ReportBugModel();
                SessionUtility session = new SessionUtility(_httpContextAccessor);

                var emailAddress = session.getSession();


                reportBugModel.ReportedBy = emailAddress;
                reportBugModel.Title = "ExportToAbaFormatError";
                reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
                reportBugModel.Description = ex.Message + "Located at line: " + ex.StackTrace;

                reportBugModel = await _reportBugService.UpsertReportBugAsync(reportBugModel);
                throw ex;

            }

        }

        [JsonExceptionFilter]
        [HttpGet]
        [Route("Xero/CheckIfConnectedToXero/{tenantId}")]
        public async Task<JsonResult> CheckIfConnectedToXero(string tenantId)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var client = new XeroClient(XeroConfig.Value);
            var ListOfSelectedTenants = await client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress));
            var Tenant = ListOfSelectedTenants.Where(t => t.TenantId.ToString().Equals(tenantId)).FirstOrDefault();

            if (Tenant == null)
            {
                return Json(new { IsConnected = false, IsError = true, Message = "Tenant is disconnected" });
            }

            return Json(new { IsConnected = true, IsError = false, Message = "Tenant is connected" });
        }

        [HttpPost]
        public async Task<IActionResult> ExportToCSVFormat([FromBody] ExportABAModel invoicePayment)
        {

            try
            {

                StringBuilder strBuilder = new();
                List<Contact> _contacts = new();
                MemoryStream stream;
                decimal totalAmount = 0;
                decimal totalCreditAmount = 0;
                decimal totalDebitAmount = 0;
                int cntInvoiceWithBankAcctDet = 0;
                var errorList = new List<ErrorIssueModel>();
                SessionUtility session = new SessionUtility(_httpContextAccessor);
                var emailAddress = session.getSession();
                var name = session.getSessionGivenName() + " " + session.getSessionFamilyName();

                if (invoicePayment.Invoices.Count > 0)
                {
                    var email = User.XeroEmail();
                    _contacts = await _xeroSvc.GetContactsAsync(emailAddress);

                    string batchType = "C";
                    string statements = invoicePayment.BankAccountName;
                    var fundsBAccountNumber = invoicePayment.BankAccountNumber;
                    string reportingMethod = "M";
                    string DDCodeOptionalField = String.Empty;
                    string optionalField = string.Empty;

                    if (invoicePayment.Invoices.Count > 4999)
                    {
                        reportingMethod = "S";
                    }


                    var dateTime = DateTime.Parse(invoicePayment.TransactionDate, System.Globalization.CultureInfo.InvariantCulture);
                    var paymentTime = string.Empty;
                    var batchCreation = string.Empty;
                    long hashtotal = 0;
                    string strhashtotal = String.Empty;
                    // 0 - Descriptive Record
                    //strBuilder.Append($"1" + batchType + dateTime.ToString("yyyy-MM-dd").Replace("-", "") + DateTime.Now.ToString("HHmm").PadLeft(4, '0') + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + fundsBAccountNumber.Replace("-", "").PadRight(15) + optionalField.PadRight(7) + reportingMethod + fundsBAccountNumber.Replace("-", "").PadRight(15) + $"BATCHPAY".PadRight(12) + optionalField.PadRight(24) + "\n");
                    strBuilder.Append($"1" + batchType + dateTime.ToString("yyyy-MM-dd").Replace("-", "") + paymentTime.PadRight(4) + batchCreation.PadRight(8) + fundsBAccountNumber.Replace("-", "").Substring(0, 15).PadRight(15) + DDCodeOptionalField.PadRight(7) + reportingMethod + fundsBAccountNumber.Replace("-", "").Substring(0, 15).PadRight(15) + $"BATCHPAY".PadRight(12) + optionalField.PadRight(36) + "\n");

                    /// 1 – Detail Record
                    foreach (var invoice in invoicePayment.Invoices)
                    {
                        invoice.Contact = _contacts.Where(w => w.ContactID == invoice.Contact?.ContactID).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(invoice.Contact.BankAccountDetails))
                        {
                            string acctDetails = invoice.Contact.BankAccountDetails.Replace("-", "");

                            //06-0475-0123456-02
                            //060475012345602
                            string branchNumber = acctDetails.Substring(3, 4);
                            string accntNumber = acctDetails.Substring(6, 7);
                            hashtotal = hashtotal + Convert.ToInt64(branchNumber + accntNumber);

                            strhashtotal = hashtotal.ToString().PadRight(11);

                            int? amount = (int?)(invoice.AmountDueEditable * 100);
                            var formattedAmt = String.Concat(Format(amount), amount?.ToString().Replace(",", "").Replace(".", "").Trim());
                            var concatValue = String.Concat("50", formattedAmt.PadLeft(10));
                            var concatValueTrim = concatValue.Replace(" ", "0");
                            var otherPartyName = invoice.Contact.Name.Length > 32 ? invoice.Contact.Name.ToUpper().Substring(0, 32).PadRight(32) : invoice.Contact.Name.ToUpper().PadRight(32);

                            strBuilder.Append($"2" + acctDetails.PadRight(17) + concatValueTrim + otherPartyName + optionalField.PadRight(72) + "\n");
                            cntInvoiceWithBankAcctDet++;
                        }


                        if (invoice.Contact.BankAccountDetails != null)
                        {

                            if (!string.IsNullOrWhiteSpace(invoice.Contact.BankAccountDetails))
                            {
                                totalAmount = totalAmount + ((invoice.AmountDue.HasValue ? invoice.AmountDue.Value : 0));
                                totalCreditAmount = totalCreditAmount + (invoice.AmountDue.HasValue ? invoice.AmountDue.Value : 0);
                                totalDebitAmount = 0;
                            }

                        }

                    }


                    string strtotalAmount = String.Format("{0:0.00}", totalAmount);
                    string strtotalCreditAmount = String.Format("{0:0.00}", totalCreditAmount);
                    string strtotalDebitAmount = String.Format("{0:0.00}", totalDebitAmount);

                    int pos = strtotalAmount.IndexOf(".");
                    int poscredit = strtotalCreditAmount.IndexOf(".");
                    int posdebit = strtotalDebitAmount.IndexOf(".");

                    if (pos >= 0)
                    {
                        strtotalAmount = strtotalAmount.Remove(pos, 1);
                    }

                    if (poscredit >= 0)
                    {
                        strtotalCreditAmount = strtotalCreditAmount.Remove(poscredit, 1);
                    }

                    if (posdebit >= 0)
                    {
                        strtotalDebitAmount = strtotalDebitAmount.Remove(posdebit, 1);
                    }


                    string Reserved1 = string.Empty;
                    string Reserved2 = string.Empty;
                    string Reserved3 = string.Empty;

                    //7 – Batch Control Record
                    //strBuilder.Append($"7999-999" + blankField.PadRight(12) + strtotalAmount.PadLeft(10, '0') + strtotalCreditAmount.PadLeft(10, '0') + strtotalDebitAmount.PadLeft(10, '0') + blankField2.PadRight(24) + $"{ invoicePayment.Invoices.Count}".PadLeft(6, '0') + blankField3.PadRight(40));
                    strBuilder.Append($"3" + strtotalAmount.PadLeft(10, '0') + strtotalDebitAmount.PadLeft(10, '0') + strtotalCreditAmount.PadLeft(10, '0') + $"{cntInvoiceWithBankAcctDet}".PadLeft(6, '0') + strhashtotal + "BATCHPAY");
                    //strBuilder.Append("3" + strtotalAmount.PadLeft(10, '0') + strtotalDebitAmount.PadLeft(10, '0'));
                }
                var data = _exportSvc.ExportToTextFile(strBuilder.ToString());
                stream = new MemoryStream(data);

                userTrack = await _apiClientSvc.GetUserIPAsync();
                userTrack.Browser = string.Empty;
                userTrack.Browser = Global.GlobalBrowser;//await JSRuntime.InvokeAsync<string>(identifier: "identifyBrowser");
                userTrack.DateTimeLog = DateTime.Now;
                userTrack.UserName = emailAddress;
                userTrack.userEmail = emailAddress;
                userTrack.Page = "Xero";
                userTrack.methodName = "ExportToCSVFormat";
                userTrack.TotalNumInvoiceTransaction = cntInvoiceWithBankAcctDet;
                userTrack.TotalInvoiceTransactionErr = invoicePayment.errorCount;
                userTrack.totalInvoiceAmount = totalAmount;

                if (Convert.ToBoolean(Int32.Parse(invoicePayment.isinsertRecordDB)) == true)
                {
                    string JsonString = JsonSerializer.Serialize(invoicePayment);
                    _xeroSvc.SaveTransactionTrack(emailAddress, "ExportDataFormat", JsonString);
                }

                await _userSvcTrack.InsertUserTrackAsync(userTrack);
                return File(stream, "text/plain", $"file_{DateTime.Now.ToShortDateString().Replace("/", "")}.txt");

            }
            catch (Exception ex)
            {
                ReportBugModel reportBugModel = new ReportBugModel();
                SessionUtility session = new SessionUtility(_httpContextAccessor);

                var emailAddress = session.getSession();

                reportBugModel.ReportedBy = emailAddress;
                reportBugModel.Title = "ExportToCSVFormatError";
                reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
                reportBugModel.Description = ex.Message + "Located at line: " + ex.StackTrace;

                reportBugModel = await _reportBugService.UpsertReportBugAsync(reportBugModel);
                throw ex;
            }

        }


        [JsonExceptionFilter]
        [HttpPost]
        public async Task<JsonResult> FilterBills([FromBody] BillsDateRangeModel dateRange)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            List<Invoice> invs = new();

            var invoicePage = await _xeroSvc.GetBillsAsync(emailAddress, dateRange.Page);
            invs.AddRange(invoicePage.Data._Invoices.OrderBy(inv => inv.Date).ToList());
            int page = 1;

            if (invs.Count > 0)
            {
                while (invs.Count % 100 == 0)
                {
                    invoicePage = await _xeroSvc.GetBillsAsync(emailAddress, page + 1);
                    if (invoicePage.Data._Invoices.Count > 0)
                    {
                        var filteredInvoices = invoicePage.Data._Invoices.OrderBy(inv => inv.Date).ToList();

                        foreach (var inv in filteredInvoices)
                        {
                            invs.Add(inv);
                        }
                    }
                    page++;
                }
            }
            var invoices = new List<InherittedInvoice2>();
            var Contacts = await _xeroSvc.GetContact(emailAddress);

            foreach (var item in invs)
            {


                var contact = Contacts._Contacts.Where(c => c.ContactID == item.Contact.ContactID).FirstOrDefault();
                invoices.Add(new InherittedInvoice2
                {
                    InvoiceID = (Guid)item.InvoiceID,
                    AmountDueEditable = item.AmountDue,
                    AmountDue = item.AmountDue,
                    AmountPaid = item.AmountPaid,
                    Contact = item.Contact,
                    LineItems = item.LineItems,
                    InvoiceNumber = item.InvoiceNumber,
                    Reference = item.Reference,
                    Date = item.Date.HasValue ? item.Date.Value.ToShortDateString() : "",
                    DueDate = item.DueDate.HasValue ? item.DueDate.Value.ToShortDateString() : "",
                    Status = item.Status.ToString(),
                    SentToContact = item.SentToContact,
                    ContactName = item.Contact.Name,
                    EmailAddress = contact.EmailAddress,
                    HasEmailAddress = string.IsNullOrEmpty(contact.EmailAddress) ? false : true
                });
            }

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "Xero";
            userTrack.methodName = "FilterBills";
            await _userSvcTrack.InsertUserTrackAsync(userTrack);
            return Json(new { invoices = invoices, Message = "Success", IsError = false });

        }


        [JsonExceptionFilter]
        [HttpGet]
        [Route("Xero/FilterBillsSelected/{tranasctionType}")]
        public async Task<JsonResult> FilterBillsSelected(string tranasctionType)
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var bills = await _xeroSvc.GetTransactionValues(emailAddress, tranasctionType);
            return Json(bills);

        }

        [JsonExceptionFilter]
        [HttpPost]
        public async Task<JsonResult> MarkBillsAsPaid([FromBody] BillsPaymentModel invoicePayment)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var invs = _xeroSvc.UpdateInvoiceStatusAsync(emailAddress, invoicePayment);
            userTrack = _apiClientSvc.GetUserIPAsync().Result;
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "Xero";
            userTrack.methodName = "MarkBillsAsPaid";
            _userSvcTrack.InsertUserTrackAsync(userTrack).Wait();
            await _xeroSvc.DeleteTransactionAsync(emailAddress, "ExportDataFormat");
            return Json(new { Invoices = invs, Message = "Success", IsError = false });

        }

        [HttpGet]
        [JsonExceptionFilter]
        public async Task<JsonResult> GetBankAccountTypes()
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var bankAccounts = await _xeroSvc.GetBankAccountsAsync(emailAddress);
            return Json(new { BankAccounts = bankAccounts, sessionTimeOut = false, IsError = false, Message = "Success" });

        }

        [JsonExceptionFilter]
        [HttpPost]
        public async Task<ActionResult> UpdatePaymentHistory([FromBody] Payments payments)
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            _xeroSvc.UpdatePaymentHistory(emailAddress, payments);
            userTrack = _apiClientSvc.GetUserIPAsync().Result;
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "Xero";
            userTrack.methodName = "UpdatePaymentHistory";
            _userSvcTrack.InsertUserTrackAsync(userTrack).Wait();
            return Json(new { Message = "Success", IsError = false });
        }

        [JsonExceptionFilter]
        [HttpPost]
        public async Task<ActionResult> SendRemittanceAdvice([FromBody] SendRemittanceModel sendRemittanceModel)
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var xeroTenantModel = _organisationService.GetCurrentOrganisationByEmail(emailAddress).Result;

            var remittances = _xeroSvc.GroupInvoicesAdviceAsync(sendRemittanceModel.selectedInvoices, emailAddress, sendRemittanceModel.payments);
            Dictionary<string, SelectPdf.PdfDocument> keyValuePairs = new Dictionary<string, SelectPdf.PdfDocument>();
            int counter = 1;
            foreach (var remittance in remittances)
            {
                string result = this.RenderViewAsync("RemittanceAdviceReport", remittance, true).Result;
                SelectPdf.HtmlToPdf converter = new SelectPdf.HtmlToPdf();
                SelectPdf.HtmlToPdfOptions options = new SelectPdf.HtmlToPdfOptions();
                converter.Options.PdfPageSize = SelectPdf.PdfPageSize.Letter;
                converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
                converter.Options.MarginTop = 20;
                converter.Options.MarginRight = 5;
                converter.Options.MarginBottom = 5;
                converter.Options.MarginLeft = 15;
                converter.Options.WebPageWidth = 800;

                var doc = converter.ConvertHtmlString(result);
                //var render = new ChromePdfRenderer();
                //var doc2 = render.RenderHtmlAsPdf(result.ToString());

                keyValuePairs.Add(remittance.Contact.ContactID.ToString(), doc);
                counter++;
            }
            await _emailSvc.SendEmailAsync(keyValuePairs, remittances, xeroTenantModel.Email, xeroTenantModel.Name);
            return Json(new { Message = "Success", IsError = false });
        }

        [JsonExceptionFilter]
        [HttpPost]
        public async Task<ActionResult> ProcessBatchReport([FromBody] SendRemittanceModel sendRemittanceModel)
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var remittances = await _xeroSvc.GroupBatchReportAsync(sendRemittanceModel.selectedInvoices, emailAddress, sendRemittanceModel.payments);
            SelectPdf.HtmlToPdf converter = new SelectPdf.HtmlToPdf();
            SelectPdf.HtmlToPdfOptions options = new SelectPdf.HtmlToPdfOptions();

            var Renderer = new ChromePdfRenderer();
            string result = await this.RenderViewAsync("BatchPaymentReport", remittances, true);
            var fileName = emailAddress + "_BatchReport.pdf";
            converter.Options.PdfPageSize = SelectPdf.PdfPageSize.Letter;
            converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
            converter.Options.MarginTop = 20;
            converter.Options.MarginRight = 5;
            converter.Options.MarginBottom = 5;
            converter.Options.MarginLeft = 15;
            converter.Options.WebPageWidth = 800;
            var fileResult = converter.ConvertHtmlString(result);
            fileResult.Save("GeneratedFiles/BatchPaymentReport/" + fileName);
            return Json(new { Message = "Success", IsError = false, fileName = fileName });
        }

        [Route("Xero/DownloadPDF/{fileName}")]
        public FileContentResult DownloadPDFtest(string fileName)
        {
            var myfile = System.IO.File.ReadAllBytes("GeneratedFiles/BatchPaymentReport/" + fileName);
            return File(myfile, "application/pdf", fileName);

        }

        [HttpGet]
        public FileContentResult DownloadPDFtest()
        {
            var myfile = System.IO.File.ReadAllBytes("formattedSendRemittance.html");
            return File(myfile, "application/pdf", "Sample.pdf");

        }

        [NonAction]
        private string Format(decimal? amount)
        {
            string amountFormat = "";
            if (amount.Value.ToString().Length < 10)
            {
                var length = 10 - amount.Value.ToString().Length;
                for (int i = 0; i < length; i++)
                {
                    amountFormat += "0";
                }
            }

            return amountFormat;
        }

        private JsonResult XeroResponseCodeHandling(Exception ex)
        {
            var result = ex.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
            if (result != null)
            {
                var ErrorCode = ((Xero.NetStandard.OAuth2.Client.ApiException)ex.InnerException).ErrorCode;

                if (ErrorCode == 400)
                {
                    var ErrorContent = ((Xero.NetStandard.OAuth2.Client.ApiException)ex.InnerException).ErrorContent;
                    return Json(new { Message = "Bad Request - A validation exception has occurred", InnerMessage = ErrorContent, IsError = true, ErrorCode = ErrorCode });
                }
                if (ErrorCode == 401)
                    return Json(new { Message = "Unauthorized", InnerMessage = "User must reauthorize xero", IsError = true, ErrorCode = ErrorCode });
                if (ErrorCode == 429)
                    return Json(new { Message = "Rate Limit Exceed", InnerMessage = "The API rate limit for your organisation/application pairing has been exceeded.", IsError = true, ErrorCode = ErrorCode });
                if (ErrorCode == 503)
                    return Json(new { Message = "Organisation offline", InnerMessage = "The organisation temporarily cannot be connected to.", IsError = true, ErrorCode = ErrorCode });
            }

            return Json(new { Message = ex.Message, InnerMessage = ex.InnerException.Message, IsError = true, ErrorCode = 1 });

        }
    }
}