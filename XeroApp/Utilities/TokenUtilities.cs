using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Net.Http;
using Xero.NetStandard.OAuth2.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.DependencyInjection;
using XeroApp.Services;
using System.Text;
using Newtonsoft.Json;

public static class TokenUtilities
{
    [Serializable]
    public struct State
    {
        public string state { get; set; }
        public State(string state)
        {
            this.state = state;
        }
    }

    private static IXeroService _xeroSvc;

    public static IXeroService XeroSvc
    {
        get
        {
            return _xeroSvc;
        }
    }

    public static void InitializedXeroService(IXeroService xeroSvc)
    {

        _xeroSvc = xeroSvc;
    }
    public static void StoreToken(XeroOAuth2Token xeroToken, string EmailAddress)
    {

        if (TokenExists(EmailAddress))
        {
            string serializedXeroToken = JsonConvert.SerializeObject(xeroToken);
            _xeroSvc.UpdateTokenStore(EmailAddress, xeroToken, serializedXeroToken);
        }
        else
        {

            string serializedXeroToken = JsonConvert.SerializeObject(xeroToken);
            _xeroSvc.SaveTokenAsync(EmailAddress, xeroToken, serializedXeroToken);
        }
    }

    public static XeroOAuth2Token GetStoredToken(string EmailAddress)
    {
        var xeroToken = new XeroOAuth2Token();

        try
        {
            if (TokenExists(EmailAddress))
            {
                var serializedXeroToken = _xeroSvc.GetStoredPayLoadByEmail(EmailAddress);
                xeroToken = JsonConvert.DeserializeObject<XeroOAuth2Token>(serializedXeroToken);
                return xeroToken;
            }

            return xeroToken;
        }
        catch (Exception)
        {

        }

        return xeroToken;
    }

    public static bool TokenExists(string EmailAddress)
    {
        return  _xeroSvc.TokenExist(EmailAddress).Result;
    }

    public static async void DestroyToken(string EmailAddress)
    {
        var tokenExists = TokenExists(EmailAddress);
        if (tokenExists)
        {
            _xeroSvc.DeleteTokenStore(EmailAddress);
            return;
        }

    }

    private class TenantId
    {
        public Guid CurrentTenantId { get; set; }
    }

    public static void StoreTenantId(Guid tenantId, string EmailAddress)
    {
        if (TokenExists(EmailAddress))
        {
            string serializedXeroToken = JsonConvert.SerializeObject(new TenantId { CurrentTenantId = tenantId });
            _xeroSvc.StoreCurrentTenant(EmailAddress, serializedXeroToken);
        }
    }

    public static Guid GetCurrentTenantId(string EmailAddress)
    {
        Guid id;
        try
        {
            if (TokenExists(EmailAddress))
            {
                var serializedXeroToken = _xeroSvc.GetCurrentTenant(EmailAddress).Result;
                id = JsonConvert.DeserializeObject<TenantId>(serializedXeroToken).CurrentTenantId;
            }
            else
            {
                return new Guid("0000");
            }

        }
        catch (IOException)
        {
            id = Guid.Empty;
        }

        return id;
    }

    public static void StoreState(string state)
    {
        State currentState = new State(state);
        string serializedState = System.Text.Json.JsonSerializer.Serialize(currentState);
        System.IO.File.WriteAllText("./state.json", serializedState);
    }

    public static string GetCurrentState()
    {
        string state;
        try
        {
            string serializedIndexFile = System.IO.File.ReadAllText("./state.json");
            state = System.Text.Json.JsonSerializer.Deserialize<State>(serializedIndexFile).state;
        }
        catch (IOException)
        {
            state = null;
        }

        return state;
    }
}

