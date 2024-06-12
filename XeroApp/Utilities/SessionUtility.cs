namespace XeroApp.Utilities
{
    public class SessionUtility
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        public SessionUtility(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void setSession(string value)
        {
            _session.SetString("EmailAddress", value);
        }

        public string getSession()
        {
            return _session.GetString("EmailAddress");

        }

        public void setSessionXeroUserID(string value)
        {
            _session.SetString("xero_userid", value);
        }

        public string getSessionXeroUserID()
        {
            return _session.GetString("xero_userid");

        }

        public void setSessionGivenName(string value)
        {
            _session.SetString("given_name", value);
        }

        public string getSessionGivenName()
        {
            return _session.GetString("given_name");

        }

        public void setSessionFamilyName(string value)
        {
            _session.SetString("family_name", value);
        }

        public string getSessionFamilyName()
        {
            return _session.GetString("family_name");

        }

        public void clearSession()
        {
             _session.Clear();

        }
    }
}
