using AutoMapper;
using Core;
using IronPdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Config;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;
using XeroApp.Extensions;
using XeroApp.Utilities;
using XeroApp.Services.ApiLogsService;

using XeroApp.Models.BusinessModels;

namespace XeroApp.Services
{
    public interface IXeroService
    {
        Task<XeroClientAppModel> GetClientAppDetailsAsync(string currentUser, string clientid);
        XeroClient XeroConfig(XeroClientApp clientApp);
        Task<bool> ClientIdExistAsync(string currentUser, string clientid);
        Task<string> GetFinancialCodeAsync(string number);
        Task AddClientAppAsync(XeroClientAppModel obj);
        Task<XeroClientAppModel> GetClientAppsAsync(string currentUser, string clientid);
        //Task<XeroOAuth2Token> AuthenticateAsync(XeroClient xeroClient, string code);
        string GenerateCodeVerifier();

        string GetStoredPayLoadByEmail(string emailAddress);

        Task<string> GetTransactionValues(string emailAddress, string transactionType);
        Task<string> GetCurrentTenant(string emailAddress);
        Task<string> GetStore(string emailAddress);

        void DeleteTokenStore(string emailAddress);
        //Task StoreCurrentTenant(string emailAddress, string payloadDataCurrentTenant);

        Task<bool> DeleteTransactionAsync(string emaillAddress, string transactionType);

        void StoreCurrentTenant(string emailAddress, string payloadDataCurrentTenant);
        Task StoreState(string emailAddress, string payloadDataState);
        //Task UpdateTokenStore(string currentUser, XeroOAuth2Token token, string payloadData);
        void UpdateTokenStore(string currentUser, XeroOAuth2Token token, string payloadData);
        Task<bool> TokenExist(string emailAddress);
        Task SaveSessionClientIdAsync(string currentUser, string clientid);
        Task<XeroSessionClientIdModel> GetSessionClientIdAsync(string currentUser);
        Task<List<Tenant>> GetTenantsAsync(XeroClient xeroClient, XeroOAuth2Token token);
        //Task SaveTokenAsync(string currentUser, XeroOAuth2Token token , string payloadData);
        void SaveTokenAsync(string currentUser, XeroOAuth2Token token, string payloadData);
        string GetTokenAsync(string currentUser);
        Task<List<Contact>> GetContactsAsync(string currentUser);
        string GetTenantIdAsync(string token, string emailAddress);
        Task<ApiResponse<Invoices>> GetBillsAsync(string currUser, int page);
        Payments UpdateInvoiceStatusAsync(string currUser, BillsPaymentModel invoicePayment);
        
        Account GetAccountInfoAsync(string currUser, string accountCode);
        Task<List<Account>> GetBankAccountsAsync(string currUser);
        List<RemittanceAdviceModel> GroupInvoicesAdviceAsync(List<InherittedInvoice2> invoices, string currUser, Payments payments);
        Task<BatchPaymentReportModel> GroupBatchReportAsync(List<InherittedInvoice2> invoices, string currUser, Payments payments);
        Task SendRemittanceAdviceAsync(List<RemittanceAdviceModel> remittances);
        Dictionary<string, PdfDocument> CreatePDFDocuments(List<RemittanceAdviceModel> remittances);
        Dictionary<string, PdfDocument> CreateBatchReportPDFDocuments(List<RemittanceAdviceModel> remittances);
        Task<Contacts> GetContact(string currUser);
        void SaveTransactionTrack(string emaillAddress, string transactionType, string transactionValues);
        Task SaveErrorTrack(string emaillAddress, string transactionValues , string errorMessage, string errorDetails);
        void UpdatePaymentHistory(string currUser, Payments payments);
        #region Utility Methods

        void InsertApiLogs(Guid TenantID, string TenantName, DateTime CreateDateUTC, string TenantType, string status, string method, string url);
        Task<bool> CheckSessionClientIdAsync(string currentUser);
        Task<bool> DeleteSessionClientIdAsync(string currentUser);
        Task<bool> CheckXeroTokenAsync(string currentUser);
        Task<bool> DeleteXeroTokenAsync(string currentUser);
        #endregion
    }

    public class XeroService : IXeroService
    {
        private readonly XeroAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IOptions<XeroConfiguration> XeroConfigApp;
        private readonly IApiLogsService _apiLogsService;

        public XeroService(
            XeroAppDbContext db,
            IMapper mapper,
            IConfiguration config,
            IApiLogsService apiLogsService,
            IOptions<XeroConfiguration> XeroConfigApp)
        {
            _db = db;
            _mapper = mapper;
            _config = config;
            _apiLogsService = apiLogsService;
            this.XeroConfigApp = XeroConfigApp;
        }

        public async Task<XeroClientAppModel> GetClientAppDetailsAsync(string currentUser, string clientid)
        {
            var clientApp = await _db.ClientApps.Where(c => c.UserName == currentUser && c.ClientId == clientid).SingleOrDefaultAsync();
            return _mapper.Map<XeroClientAppModel>(clientApp);
        }

        public XeroClient XeroConfig(XeroClientApp clientApp)
        {
            XeroConfiguration xconfig = new XeroConfiguration
            {
                ClientId = clientApp.ClientId,
                CallbackUri = new Uri(clientApp.CallbackUri),
                Scope = clientApp.Scope,
                State = clientApp.State
            };

            return new XeroClient(xconfig);
        }

        public async Task<bool> ClientIdExistAsync(string currentUser, string clientid)
        {
            var clientApp = await _db.ClientApps.Where(p => p.UserName == currentUser && p.ClientId == clientid).ToListAsync();
            return clientApp.Count >= 1 ? true : false;
        }

        public async Task AddClientAppAsync(XeroClientAppModel obj)
        {
            var clientApp = new XeroClientAppModel
            {
                UserName = obj.UserName,
                ClientId = obj.ClientId,
                CallbackUri = obj.CallbackUri,
                Scope = _config.GetSection("Scopes").Value,
                State = "123",
                DateCreated = DateTime.Now
            };

            var client = _mapper.Map<XeroClientApp>(clientApp);

            _db.ClientApps.Add(client);
            await _db.SaveChangesAsync();
        }

        public async Task<XeroClientAppModel> GetClientAppsAsync(string currentUser, string clientid)
        {
            var clientApp = await _db.ClientApps.SingleAsync(c => c.UserName == currentUser && c.ClientId == clientid);
            return _mapper.Map<XeroClientAppModel>(clientApp);
        }

        //public async Task<XeroOAuth2Token> AuthenticateAsync(XeroClient xeroClient, string code)
        //	=> (XeroOAuth2Token)await xeroClient.RequestAccessTokenPkceAsync(code, TokenUtilities.GetCodeVerifier());

        public string GenerateCodeVerifier()
        {
            var validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-._~";
            Random random = new Random();
            int charsLength = random.Next(43, 128);
            char[] randomChars = new char[charsLength];
            for (int i = 0; i < charsLength; i++)
            {
                randomChars[i] = validChars[random.Next(0, validChars.Length)];
            }
            string codeVerifier = new String(randomChars);

            return codeVerifier;
        }

        public async Task SaveSessionClientIdAsync(string currentUser, string clientid)
        {
            _db.SessionClientIds.Add(new XeroSessionClientId { UserName = currentUser, ClientId = clientid, DateCreated = DateTime.Now });
            await _db.SaveChangesAsync();
        }

        public async Task<XeroSessionClientIdModel> GetSessionClientIdAsync(string currentUser)
        {
            var clientId = await _db.SessionClientIds.SingleAsync(c => c.UserName == currentUser);
            return _mapper.Map<XeroSessionClientIdModel>(clientId);
        }

        public async Task<List<Tenant>> GetTenantsAsync(XeroClient xeroClient, XeroOAuth2Token token)
            => await xeroClient.GetConnectionsAsync(token);

        //public async Task SaveTokenAsync(string currentUser, XeroOAuth2Token token , string payloadData)
        //{
        //          using (var transaction = _db.Database.BeginTransaction())
        //          {
        //		var tokenInfo = new XeroTokenModel
        //		{
        //			UserName = currentUser,
        //			AccessToken = token.AccessToken,
        //			RefreshToken = token.RefreshToken,
        //			IdToken = token.IdToken,
        //			ExpiresAtUtc = token.ExpiresAtUtc

        //		};

        //		var mappedEntity = _mapper.Map<XeroToken>(tokenInfo);
        //		mappedEntity.DateCreated = DateTime.Now;
        //		mappedEntity.PayLoadData = payloadData;

        //		_db.XeroTokens.Add(mappedEntity);
        //		await _db.SaveChangesAsync();
        //		transaction.Commit();
        //	}

        //}

        public void SaveTokenAsync(string currentUser, XeroOAuth2Token token, string payloadData)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var tokenInfo = new XeroTokenModel
                {
                    UserName = currentUser,
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken,
                    IdToken = token.IdToken,
                    ExpiresAtUtc = token.ExpiresAtUtc

                };

                var mappedEntity = _mapper.Map<XeroToken>(tokenInfo);
                mappedEntity.DateCreated = DateTime.Now;
                mappedEntity.PayLoadData = payloadData;

                _db.XeroTokens.Add(mappedEntity);
                _db.SaveChanges();
                transaction.Commit();
            }

        }

        public string GetTokenAsync(string currentUser)
        {
            var xeroToken = TokenUtilities.GetStoredToken(currentUser);
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var clientExpiry = new XeroClient(XeroConfigApp.Value);
                xeroToken = (XeroOAuth2Token) clientExpiry.RefreshAccessTokenAsync(xeroToken).Result;
                string emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                TokenUtilities.StoreToken(xeroToken, emailAddress);
            }
            return TokenUtilities.GetStoredToken(currentUser).AccessToken;

        }

        public async Task<string> GetFinancialCodeAsync(string number)
        {
            var token = await _db.FinancialInstitutionCodes.SingleAsync(c => c.Number == number);
            return token.Code;
        }

        public async Task<List<Contact>> GetContactsAsync(string currentUser)
        {
            var accountingApi = new AccountingApi();
            var accessToken = GetTokenAsync(currentUser);
            var tenantId = GetTenantIdAsync(accessToken, currentUser);
            var contacts = await accountingApi.GetContactsAsync(accessToken, tenantId);

            InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                                Global.globallistTenantLog[0].TenantName,
                                DateTime.Now,
                                Global.globallistTenantLog[0].TenantType,
                                "200", "Get", accountingApi.Configuration.BasePath + "/Contacts");


            return contacts._Contacts;
        }

        public string GetTenantIdAsync(string token, string currUser)
        {
            return TokenUtilities.GetCurrentTenantId(currUser).ToString();
        }

        public async Task<ApiResponse<Invoices>> GetBillsAsync(string currUser, int page)
        {
            try
            {
                var accountingApi = new AccountingApi();
                var accessToken = GetTokenAsync(currUser);
                var tenantId = GetTenantIdAsync(accessToken, currUser);
                var where = $"Type==\"ACCPAY\" AND Status==\"AUTHORISED\"";

                InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                   Global.globallistTenantLog[0].TenantName,
                   DateTime.Now,
                   Global.globallistTenantLog[0].TenantType,
                   "200", "Get", accountingApi.Configuration.BasePath + "/Invoices?where=" + where);

                return await accountingApi.GetInvoicesAsyncWithHttpInfo(accessToken, tenantId, null, where, null, null, null, null, null, page);


            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public Payments UpdateInvoiceStatusAsync(string currUser, BillsPaymentModel invoicePayment)
        {
            var accountingApi = new AccountingApi();
            var accessToken =  GetTokenAsync(currUser);
            //var accessToken = string.Empty;
            var tenantId = GetTenantIdAsync(accessToken, currUser);
            var payments = new Payments();
            var paymentsList = new List<Payment>();
            var dateTime = DateTime.Parse(invoicePayment.TransactionDate, System.Globalization.CultureInfo.InvariantCulture);
            int counterForDelay = 0;
            var account = new Account();
            var accountInfo = GetAccountInfoAsync(currUser, invoicePayment.BankAccountCode);

            if (accountInfo != null)
                account.AccountID = accountInfo.AccountID;

            account.BankAccountNumber = invoicePayment.BankAccountNumber;

            foreach (var inv in invoicePayment.Invoices)
            {
                var invoice = new Invoice();
                invoice.InvoiceID = inv.InvoiceID;
                var payment = new Payment();
                payment.Invoice = invoice;
                payment.Account = account;
                payment.Amount = inv.AmountDueEditable;
                payment.Date = dateTime;
                paymentsList.Add(payment);
                counterForDelay += 1;
            }

            payments._Payments = paymentsList;
            var paymentResult = accountingApi.CreatePaymentsAsync(accessToken, tenantId, payments, true).Result;
            //Task.Delay(30000).Wait();

            InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                              Global.globallistTenantLog[0].TenantName,
                              DateTime.Now,
                              Global.globallistTenantLog[0].TenantType,
                              "200", "Put", accountingApi.Configuration.BasePath + "/Payments");



            string JsonString = JsonSerializer.Serialize(paymentResult);
            SaveTransactionTrack(currUser, "MarkAsPaidResult", JsonString);
            return paymentResult;
        }

        public Account GetAccountInfoAsync(string currUser, string accountCode)
        {
            var accountingApi = new AccountingApi();
            var accessToken = GetTokenAsync(currUser);
            var tenantId =  GetTenantIdAsync(accessToken, currUser);
            var where = $"Code==\"{accountCode}\"";

            var account = accountingApi.GetAccountsAsync(accessToken, tenantId, null, where).Result;

            InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                         Global.globallistTenantLog[0].TenantName,
                         DateTime.Now,
                         Global.globallistTenantLog[0].TenantType,
                         "200", "Get", accountingApi.Configuration.BasePath + "/Accounts?where=" + where);


            return account._Accounts.First();
        }

        public async Task<List<Account>> GetBankAccountsAsync(string currUser)
        {
            var accountingApi = new AccountingApi();
            var accessToken = GetTokenAsync(currUser);
            var tenantId = GetTenantIdAsync(accessToken, currUser);
            var where = $"BankAccountType==\"BANK\"";

            var account = await accountingApi.GetAccountsAsync(accessToken, tenantId, null, where);


            InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                           Global.globallistTenantLog[0].TenantName,
                           DateTime.Now,
                           Global.globallistTenantLog[0].TenantType,
                           "200", "Get", accountingApi.Configuration.BasePath + "/Accounts?where=" + where);


            return account._Accounts.ToList();
        }

        public List<RemittanceAdviceModel> GroupInvoicesAdviceAsync(List<InherittedInvoice2> invoices, string currUser, Payments payments)
        {
                var groupedInvoices = invoices.GroupBy(i => i.Contact?.ContactID);
                var accountingApi = new AccountingApi();
                var accessToken = GetTokenAsync(currUser);
                var tenantId = GetTenantIdAsync(accessToken, currUser);

                List<RemittanceAdviceModel> remittanceAdviceModelList = new();
                var organizationInfo = accountingApi.GetOrganisationsAsync(accessToken, tenantId).Result._Organisations.FirstOrDefault();

                InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                            Global.globallistTenantLog[0].TenantName,
                            DateTime.Now,
                            Global.globallistTenantLog[0].TenantType,
                            "200", "Get", accountingApi.Configuration.BasePath + "/Organisation");


                var companyAddress = organizationInfo.Addresses.FirstOrDefault();
                var listOfContacts = accountingApi.GetContactsAsync(accessToken, tenantId).Result._Contacts.ToList();

                InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                             Global.globallistTenantLog[0].TenantName,
                             DateTime.Now,
                             Global.globallistTenantLog[0].TenantType,
                             "200", "Get", accountingApi.Configuration.BasePath + "/Contacts");


                foreach (var grpInvoice in groupedInvoices)
                {
                    RemittanceAdviceModel remittanceAdviceModel = new();
                    remittanceAdviceModel.ContactId = grpInvoice.Key;
                    remittanceAdviceModel.Invoices = new();
                    var newContactID = new Guid(grpInvoice.Key.ToString());
                    //remittanceAdviceModel.Contact = accountingApi.GetContactAsync(accessToken, tenantId, newContactID).Result._Contacts.FirstOrDefault();
                    remittanceAdviceModel.Contact = listOfContacts.Where(c => c.ContactID == newContactID).FirstOrDefault();
                    remittanceAdviceModel.MyCompanyName = (organizationInfo != null) ? organizationInfo.Name : string.Empty;
                    remittanceAdviceModel.MyCompanyAddress1 = (companyAddress != null) ? companyAddress.AddressLine1 : string.Empty;
                    remittanceAdviceModel.MyCompanyAddress2 = (companyAddress != null) ? companyAddress.AddressLine2 : string.Empty;
                    remittanceAdviceModel.MyCompanyAddress3 = (companyAddress != null) ? companyAddress.AddressLine3 : string.Empty;
                    remittanceAdviceModel.MyCompanyCity = (companyAddress != null) ? companyAddress.City : string.Empty;
                    remittanceAdviceModel.PostalCode = (companyAddress != null) ? companyAddress.PostalCode : string.Empty;
                    remittanceAdviceModel.ContactEmailAddress = (remittanceAdviceModel.Contact != null) ? remittanceAdviceModel.Contact.EmailAddress : string.Empty;
                    remittanceAdviceModel.ContactName = (remittanceAdviceModel.Contact != null) ? remittanceAdviceModel.Contact.Name : string.Empty;
                    remittanceAdviceModel.Total = grpInvoice.Sum(i => i.AmountDueEditable.Value);
                    remittanceAdviceModel.StillOwning = grpInvoice.Sum(i => i.AmountDue.Value) - grpInvoice.Sum(i => i.AmountDueEditable.Value);

                foreach (var inv in grpInvoice)
                {
                    var payment = payments._Payments.Where(p => p.Invoice.InvoiceID == inv.InvoiceID).FirstOrDefault();
                    inv.PaymentDate = payment.Date.ToString().Substring(0, 10);
                    inv.SendDate = DateTime.Now.ToString("dd'/'MM'/'yyyy");
                    remittanceAdviceModel.Invoices.Add(inv);
                }

                remittanceAdviceModelList.Add(remittanceAdviceModel);
                }
                return remittanceAdviceModelList;
        }

        public void InsertApiLogs(Guid TenantID, string TenantName, DateTime CreateDateUTC, string TenantType, string status, string method, string url)
        {
            var apiLogsModel = new ApiLogsModel();

            apiLogsModel.TenantId = TenantID;
            apiLogsModel.TenantName = TenantName;
            apiLogsModel.CreateDateUTC = CreateDateUTC;
            apiLogsModel.TenantType = TenantType;
            apiLogsModel.status = status;
            apiLogsModel.method = method;
            apiLogsModel.url = url;

            _apiLogsService.UpsertApiLogsAsync(apiLogsModel);
        }
        public async Task SendRemittanceAdviceAsync(List<RemittanceAdviceModel> remittances)
        {
            foreach (var remittance in remittances)
            {
            }
        }

        
        public Dictionary<string, PdfDocument> CreatePDFDocuments(List<RemittanceAdviceModel> remittances)
        {

            try
            {
                List<PdfDocument> pdfDocs = new();
                Dictionary<string, PdfDocument> keyValuePairs = new Dictionary<string, PdfDocument>();
                int counter = 1;
                decimal total = 0, stillOwing = 0;

                foreach (var remittance in remittances)
                {
                    StringBuilder strDoc = new();
                    strDoc.Append(
                        "<table id='invoices' style='width: 100%; font-size: 14px; font-family: Arial;'>" +
                        "<thead>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '><h3>REMITTANCE ADVICE</h3></th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>Payment Date</th>" +
                            "<th style='text - align: right; height: 18px; '><code>" + $"{remittance.MyCompanyName}" + "</code></th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.Invoices.FirstOrDefault().PaymentDate}" + "</code></th>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.MyCompanyAddress1}" + "</code></th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.Contact.Name}" + "</code></th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '><code>" + $"{remittance.MyCompanyAddress2}" + "</code></th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.Contact.Addresses[0].AttentionTo}" + "</code></th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>Sent Date</th>" +
                            "<th style='text - align: right; height: 18px; '><code>" + $"{remittance.MyCompanyCity + " " + remittance.PostalCode}" + "</code></th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.Contact.Addresses[0].AddressLine1}" + "</code></th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '><code>" + $"{remittance.Invoices.FirstOrDefault().SendDate}" + "</code></th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.Contact.Addresses[0].AddressLine2}" + "</code></th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '><code>" + $"{remittance.Contact.Addresses[0].Country}" + "</code></th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>Tax Reg</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: right; height: 18px; '><code>" + $"{remittance.Contact.TaxNumber}" + "</code></th>" +
                            "<th style='text - align: right; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                            "<th style='text - align: left; height: 18px; '>&nbsp;</th>" +
                        "</tr>" +
                        "<tr>" +
                            "<th style='text-align: left;'>Invoice Date</th>" +
                            "<th style='text-align: left;'>Reference</th>" +
                            "<th style='text-align: right;'>Invoice Total</th>" +
                            "<th style='text-align: right;'>Amount Paid</th>" +
                            "<th style='text-align: right;'>Still Owing</th>" +
                        "</tr>" +
                        "</thead>" +
                        "<tbody>");

                    foreach (var invoice in remittance.Invoices)
                    {
                        strDoc.Append(
                            "<tr>" +
                                "<td style='text-align: left;'><code>" + $"{invoice.Date}" + "</code></td>" +
                                "<td style='text-align: left;'><code>" + $"{invoice.Reference}" + "</code></td>" +
                                "<td style='text-align: right;'><code>" + $"{invoice.AmountDue}" + "</code></td>" +
                                "<td style='text-align: right;'><code>" + $"{invoice.AmountDueEditable}" + "</code></td>" +
                                "<td style='text-align: right;'><code>" + $"{invoice.AmountDue - invoice.AmountDueEditable}" + "</code></td>" +
                            "</tr>"
                        );
                    }
                    total = remittance.Invoices.Sum(i => i.AmountDueEditable.Value);

                    stillOwing = remittance.Invoices.Sum(i => i.AmountDue.Value) - remittance.Invoices.Sum(i => i.AmountDueEditable.Value);
                    strDoc.Append(
                        "</tbody>" +
                        "<tfoot style='text-align: right;'>" +
                            "<tr>" +
                                "<td colspan='3'>" + "<b>Total AUD</b>" + "</td>" +
                                "<td colspan='1'>" + $"<b>{total}</b>" + "</td>" +
                                "<td colspan='1'>" + $"<b>{stillOwing}</b>" + "</td>" +
                            "</tr>" +
                        "</tfoot>"
                    );
                    strDoc.Append("</table>");

                    var render = new ChromePdfRenderer();

                    var doc = render.RenderHtmlAsPdf(strDoc.ToString());
                    keyValuePairs.Add(remittance.Contact.EmailAddress, doc);
                    counter++;
                }

                return keyValuePairs;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        #region Utility Methods
        public async Task<bool> CheckSessionClientIdAsync(string currentUser)
        {
            var currSessionClientId = await _db.SessionClientIds.Where(c => c.UserName == currentUser).ToListAsync();
            return currSessionClientId.Count > 0 ? true : false;
        }

        public async Task<bool> DeleteTransactionAsync(string emaillAddress, string transactionType)
        {
            var transactionResult = await _db.TransactionTrack.Where(c => c.EmailAddress == emaillAddress && c.TransactionType == transactionType).FirstOrDefaultAsync();

            _db.TransactionTrack.RemoveRange(transactionResult);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSessionClientIdAsync(string currentUser)
        {
            var currSessionClientId = await _db.SessionClientIds.Where(c => c.UserName == currentUser).ToListAsync();

            _db.SessionClientIds.RemoveRange(currSessionClientId);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckXeroTokenAsync(string currentUser)
        {
            var tokens = await _db.XeroTokens.Where(t => t.UserName == currentUser).ToListAsync();
            return tokens.Count > 0 ? true : false;
        }

        public async Task<bool> DeleteXeroTokenAsync(string currentUser)
        {
            var currXeroTokens = await _db.XeroTokens.Where(c => c.UserName == currentUser).ToListAsync();

            _db.XeroTokens.RemoveRange(currXeroTokens);
            await _db.SaveChangesAsync();
            return true;
        }

        public string GetStoredPayLoadByEmail(string emailAddress)
        {
            var currPayLoad = _db.XeroTokens.Where(c => c.UserName == emailAddress).Select(c => c.PayLoadData).FirstOrDefault();
            return currPayLoad;
        }


        public async Task<bool> TokenExist(string emailAddress)
        {
            var currPayLoad = await _db.XeroTokens.FirstOrDefaultAsync(c => c.UserName == emailAddress);

            return (currPayLoad != null) ? true : false;
        }



        public void DeleteTokenStore(string emailAddress)
        {

            using (var transaction = _db.Database.BeginTransaction())
            {

                var currXeroTokens = _db.XeroTokens.Where(c => c.UserName == emailAddress).ToList();
                _db.XeroTokens.RemoveRange(currXeroTokens);
                _db.SaveChanges();
                transaction.Commit();
            }
        }

        public void UpdateTokenStore(string currentUser, XeroOAuth2Token token, string payloadData)
        {
            try
            {
                using (var transaction = _db.Database.BeginTransaction())
                {

                    var currXeroTokens = _db.XeroTokens.FirstOrDefault(c => c.UserName == currentUser);
                    currXeroTokens.AccessToken = token.AccessToken;
                    currXeroTokens.RefreshToken = token.RefreshToken;
                    currXeroTokens.IdToken = token.IdToken;
                    currXeroTokens.ExpiresAtUtc = token.ExpiresAtUtc;
                    currXeroTokens.PayLoadData = payloadData;
                    _db.SaveChanges();
                    transaction.Commit();
                }
            } catch (Exception ex)
            {
                throw ex;
            }

        }
        public void StoreCurrentTenant(string emailAddress, string payloadDataCurrentTenant)
        {
            try
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var currXeroTokens = _db.XeroTokens.SingleOrDefault(c => c.UserName == emailAddress);
                    currXeroTokens.CurrentTenantId = payloadDataCurrentTenant;
                    _db.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> GetCurrentTenant(string emailAddress)
        {
            //System.Threading.Thread.Sleep(100);
            var curTenantPayLoad = await _db.XeroTokens.Where(c => c.UserName == emailAddress).Select(c => c.CurrentTenantId).FirstOrDefaultAsync();
            return curTenantPayLoad;
        }

        public async Task StoreState(string emailAddress, string payloadDataState)
        {
            try
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var currXeroTokens = await _db.XeroTokens.SingleOrDefaultAsync(c => c.UserName == emailAddress);
                    currXeroTokens.State = payloadDataState;
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<string> GetTransactionValues(string emailAddress, string transactionType)
        {
            //System.Threading.Thread.Sleep(100);
            try
            {
                var transactionValues = await _db.TransactionTrack.Where(c => c.EmailAddress == emailAddress && c.TransactionType == transactionType).Select(c => c.TransactionValues).FirstOrDefaultAsync();
                return transactionValues;
            }
            catch (Exception ex) {

                return ex.Message;
            }

        }

        public async Task<string> GetStore(string emailAddress)
        {
            //System.Threading.Thread.Sleep(100);
            var curStatePayLoad = await _db.XeroTokens.Where(c => c.UserName == emailAddress).Select(c => c.State).FirstOrDefaultAsync();
            return curStatePayLoad;
        }

        public Dictionary<string, PdfDocument> CreateBatchReportPDFDocuments(List<RemittanceAdviceModel> remittances)
        {
            throw new NotImplementedException();
        }

        public async Task<BatchPaymentReportModel> GroupBatchReportAsync(List<InherittedInvoice2> invoices, string currUser, Payments payments)
        {
            try
            {
                var groupedInvoices = invoices.GroupBy(i => i.PaymentDate);
                var accountingApi = new AccountingApi();
                var accessToken = GetTokenAsync(currUser);
                var tenantId = GetTenantIdAsync(accessToken, currUser);

                BatchPaymentReportModel batchPaymentReportModel = new();
                var organizationInfo = accountingApi.GetOrganisationsAsync(accessToken, tenantId).Result._Organisations.FirstOrDefault();
                if (organizationInfo != null)
                {
                    batchPaymentReportModel.MyCompanyName = organizationInfo.Name;
                    var companyAddress = organizationInfo.Addresses.FirstOrDefault();

                    if (companyAddress != null)
                    {
                        batchPaymentReportModel.MyCompanyAddress1 = companyAddress.AddressLine1;
                        batchPaymentReportModel.MyCompanyAddress2 = companyAddress.AddressLine2;
                        batchPaymentReportModel.MyCompanyAddress3 = companyAddress.AddressLine3;
                        batchPaymentReportModel.MyCompanyCityPostal = companyAddress.PostalCode;

                    }
                }

                InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                             Global.globallistTenantLog[0].TenantName,
                             DateTime.Now,
                             Global.globallistTenantLog[0].TenantType,
                             "200", "Get", accountingApi.Configuration.BasePath + "/Organisation");


                batchPaymentReportModel.Payments = payments;
                var accountGUID = new Guid(payments._Payments.FirstOrDefault().Account.AccountID.ToString());
                var bank = accountingApi.GetAccountAsync(accessToken, tenantId, accountGUID).Result._Accounts;


                InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                        Global.globallistTenantLog[0].TenantName,
                        DateTime.Now,
                        Global.globallistTenantLog[0].TenantType,
                        "200", "Get", accountingApi.Configuration.BasePath + "/Accounts/" + accountGUID);


                if (bank != null && bank.Any())
                {
                    batchPaymentReportModel.BankAccountNumber = bank.Select(b => b.BankAccountNumber).FirstOrDefault();
                    batchPaymentReportModel.BankAccount = bank.Select(b => b.Name).FirstOrDefault();
                }
                return batchPaymentReportModel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Contacts> GetContact(string currUser)
        {
            var accountingApi = new AccountingApi();
            var accessToken = GetTokenAsync(currUser);
            var tenantId = GetTenantIdAsync(accessToken, currUser);

            var contacts = await accountingApi.GetContactsAsync(accessToken, tenantId, null, null, null, null, null, null, null, null);

            InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                                Global.globallistTenantLog[0].TenantName,
                                DateTime.Now,
                                Global.globallistTenantLog[0].TenantType,
                                "200", "Get", accountingApi.Configuration.BasePath + "/Contacts");

            return contacts;
        }

        public void SaveTransactionTrack(string emaillAddress, string transactionType, string transactionValues)
        {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var transactionTrack = new TransactionTrack{ 
                        EmailAddress = emaillAddress, 
                        TransactionType = transactionType, 
                        TransactionValues = transactionValues, 
                        DateCreated = DateTime.Now 
                    };
                    _db.TransactionTrack.Add(transactionTrack);
                    _db.SaveChanges();
                    transaction.Commit();
                }
        }

        public async Task SaveErrorTrack(string emaillAddress, string transactionValues, string errorMessage, string errorDetails)
        {
            try
            {
                using (var transaction = _db.Database.BeginTransaction())
                {
                    var errorTrack = new ErrorTrack
                    {
                        EmailAddress = emaillAddress,
                        TransactionValues = transactionValues,
                        ErrorMessage = errorMessage,
                        ErrorMessageDetail = errorDetails,
                        DateCreated = DateTime.Now
                    };
                    _db.ErrorTrack.Add(errorTrack);
                    await _db.SaveChangesAsync();
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void UpdatePaymentHistory(string currUser, Payments payments)
        {
            var historyRecord = new HistoryRecord();
            historyRecord.Details = "Paid by BatchPay.com.au";
            historyRecord.User = currUser;

            var historyRecords = new HistoryRecords();
            var historyRecordsList = new List<HistoryRecord>();
            historyRecordsList.Add(historyRecord);
            historyRecords._HistoryRecords = historyRecordsList;

            var accountingApi = new AccountingApi();
            var accessToken = GetTokenAsync(currUser);
            var tenantId =  GetTenantIdAsync(accessToken, currUser);
            int counterForDelay = 0;

   
            if (payments._Payments.Count <= 150 && payments._Payments.Count >= 75)
            {
                Task.Delay(30000).Wait();
                for (int i = 0; i < payments._Payments.Count; i++)
                {
                    if (counterForDelay > 0 && counterForDelay % 50 == 0)
                    {
                        Task.Delay(60000).Wait();
                        counterForDelay = 0;
                    }

                    var invoiceId = payments._Payments[i].Invoice.InvoiceID.ToString();
                    var result = accountingApi.CreateInvoiceHistoryAsync(accessToken, tenantId, new Guid(invoiceId), historyRecords).Result;
                    
                    InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                                                    Global.globallistTenantLog[0].TenantName,
                                                    DateTime.Now,
                                                    Global.globallistTenantLog[0].TenantType,
                                                    "200", "Get", accountingApi.Configuration.BasePath + "/Invoices/" + invoiceId + "/History");


                    counterForDelay++;
                }
            }

            if (payments._Payments.Count <= 75)
            {
                Task.Delay(30000).Wait();
                for (int i = 0; i < payments._Payments.Count; i++)
                {
                    if (counterForDelay > 0 && counterForDelay % 50 == 0)
                    {
                        Task.Delay(40000).Wait();
                        counterForDelay = 0;
                    }

                    var invoiceId = payments._Payments[i].Invoice.InvoiceID.ToString();
                    var result = accountingApi.CreateInvoiceHistoryAsync(accessToken, tenantId, new Guid(invoiceId), historyRecords).Result;
                    counterForDelay++;
                }
            }
        }
        #endregion
    }
}