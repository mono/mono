//------------------------------------------------------------------------------
// <copyright file="ProxyHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Runtime.Serialization;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Xml;
    using System.Collections.ObjectModel;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Web.Util;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    internal static class ProxyHelper
    {

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static object CreateWebRequestAndGetResponse(string serverUri, ref CookieContainer cookies,
                                                              string username, string connectionString,
                                                              string connectionStringProvider,
                                                              string [] paramNames, object [] paramValues,
                                                              Type returnType)
        {


            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(serverUri);
            request.UseDefaultCredentials = true;
            request.ContentType = "application/json; charset=utf-8";
            request.AllowAutoRedirect = true;
            request.Method = "POST";

            if (cookies == null)
                cookies = ConstructCookieContainer(serverUri, username,
                                                   connectionString, connectionStringProvider);
            if (cookies != null)
                request.CookieContainer = cookies;

            if (paramNames != null && paramNames.Length > 0) {
                byte [] postedBody = GetSerializedParameters(paramNames, paramValues);
                request.ContentLength = postedBody.Length;
                using(Stream s = request.GetRequestStream()) {
                    s.Write(postedBody, 0, postedBody.Length);
                }
            } else {
                request.ContentLength = 0;
            }

            // Get the response
            try {
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
                    if (response == null)
                        throw new WebException(AtlasWeb.ClientService_BadJsonResponse);

                    GetCookiesFromResponse(response, cookies, serverUri, username, connectionString, connectionStringProvider);
                    if (returnType == null)
                        return null;

                    JavaScriptTypeResolver resolver = AppSettings.UseLegacyClientServicesJsonHandling ? (JavaScriptTypeResolver) new SimpleTypeResolver() : (JavaScriptTypeResolver) new DictionaryTypeResolver();
                    JavaScriptSerializer        js            = new JavaScriptSerializer(resolver);
                    string                      responseJson  = GetResponseString(response);
                    Dictionary<string, object>  wrapperObject = js.DeserializeObject(responseJson) as Dictionary<string, object>;

                    if (wrapperObject == null || !wrapperObject.ContainsKey("d")) {
                        throw new WebException(AtlasWeb.ClientService_BadJsonResponse);
                    }

                    return ObjectConverter.ConvertObjectToType(wrapperObject["d"], returnType, js);

                }
            } catch(WebException we) {
                HttpWebResponse response = (HttpWebResponse) we.Response;
                if (response == null)
                    throw;
                throw new WebException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.ProxyHelper_BadStatusCode, response.StatusCode.ToString(), GetResponseString(response)), we);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static void GetCookiesFromResponse(HttpWebResponse response, CookieContainer cookies, string serverUri, string username,
                                                   string connectionString, string connectionStringProvider) {
            foreach (Cookie c in response.Cookies)
                cookies.Add(c);

            int numHeaders = response.Headers.Count;
            for(int iter=0; iter<numHeaders; iter++) {
                string header = response.Headers.GetKey(iter);
                if (header != null && header == "Set-Cookie") {
                    string cookieValue = response.Headers.Get(iter);
                    StoreCookie(serverUri, cookieValue, username, connectionString, connectionStringProvider);
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static byte[] GetSerializedParameters(string[] paramNames, object[] paramValues)
        {
            int len = paramNames.Length;
            if (len != paramValues.Length)
                throw new ArgumentException(null, "paramValues");
            if (len < 1)
                return new byte[0];

            StringBuilder           sb = new StringBuilder(40 * len);
            JavaScriptSerializer    js = new JavaScriptSerializer();

            sb.Append("{" + js.Serialize(paramNames[0]) + ":" + js.Serialize(paramValues[0]));
            for (int iter = 1; iter < len; iter++)
                sb.Append("," + js.Serialize(paramNames[iter]) + ":" + js.Serialize(paramValues[iter]));
            sb.Append("}");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static string GetResponseString(HttpWebResponse response)
        {
            using(Stream s = response.GetResponseStream()) {
                using (StreamReader readStream = new StreamReader(s, Encoding.UTF8)) {
                    int len = 1024;
                    if (s.CanSeek && s.Length > len)
                        len = (int) s.Length;

                    char[]          read    = new char[len];
                    StringBuilder   sb      = new StringBuilder(len);

                    int count = readStream.Read(read, 0, len);
                    while (count > 0) {
                        sb.Append(new string(read, 0, count));
                        count = readStream.Read(read, 0, len);
                    }
                    return sb.ToString();
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
#if ENABLE_WCF_SUPPORT
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        internal static CustomBinding GetBinding()
        {
            HttpTransportBindingElement be = new HttpTransportBindingElement();
            be.AllowCookies = false;
            if (Thread.CurrentPrincipal != null && Thread.CurrentPrincipal.Identity is WindowsIdentity)
                be.AuthenticationScheme = AuthenticationSchemes.Negotiate;
            TextMessageEncodingBindingElement tmbe = new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);
            CustomBinding binding = new CustomBinding(tmbe, be);
            return binding;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        internal static void GetCookiesFromWCF(CookieContainer authenticationCookies, string serverUri, string username, string connectionString, string connectionStringProvider)
        {
            if (username == null) {
                if (Thread.CurrentPrincipal != null)
                    username = Thread.CurrentPrincipal.Identity.Name;
                else
                    username = string.Empty;
            }

            HttpResponseMessageProperty httpResponseProperty = (HttpResponseMessageProperty)OperationContext.Current.IncomingMessageProperties[HttpResponseMessageProperty.Name];
            if (httpResponseProperty == null || httpResponseProperty.Headers == null || httpResponseProperty.Headers.Count < 1)
                return;

            int count = httpResponseProperty.Headers.Count;
            Uri uri = ((authenticationCookies==null) ? null : new Uri(serverUri));

            for(int iter=0; iter<count; iter++) {
                string key = httpResponseProperty.Headers.Keys[iter];
                if (key == null || key != "Set-Cookie")
                    continue;
                StoreCookie(serverUri, httpResponseProperty.Headers[iter], username, connectionString, connectionStringProvider);
                if (authenticationCookies != null)
                    authenticationCookies.SetCookies(uri, httpResponseProperty.Headers[iter]);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        internal static void AddCookiesToWCF(CookieContainer authenticationCookies, string serverUri,
                                             string username, string connectionString, string connectionStringProvider)
        {
            StringBuilder allCookies = null;
            if (authenticationCookies != null) {
                CookieCollection cookies = authenticationCookies.GetCookies(new Uri(serverUri));
                if (cookies == null || cookies.Count < 1)
                    return;
                foreach (Cookie c in cookies) {
                    if (allCookies == null)
                        allCookies = new StringBuilder(c.ToString());
                    else
                        allCookies.Append("; " + c.ToString());
                }
            } else {
                if (username == null) {
                    if (Thread.CurrentPrincipal != null)
                        username = Thread.CurrentPrincipal.Identity.Name;
                    else
                        username = string.Empty;
                }
                string [] cookies = GetCookiesFromIECache(serverUri, username, connectionString, connectionStringProvider);
                if (cookies == null || cookies.Length < 1)
                    return;
                foreach (string c in cookies) {
                    string cookie = ((c == null) ? null : c.Trim());
                    if (!string.IsNullOrEmpty(cookie)) {
                        if (allCookies == null)
                            allCookies = new StringBuilder(cookie);
                        else
                            allCookies.Append("; " + cookie);
                    }
                }
            }
            if (allCookies != null)
                AddCookiesToWCFOperationContext(allCookies.ToString());
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        private static void AddCookiesToWCFOperationContext(string cookies)
        {
            HttpRequestMessageProperty httpRequestProperty = null;
            if (OperationContext.Current.OutgoingMessageProperties.ContainsKey(HttpRequestMessageProperty.Name))
                httpRequestProperty = OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;

            if (httpRequestProperty == null) {
                httpRequestProperty = new HttpRequestMessageProperty();
                OperationContext.Current.OutgoingMessageProperties.Add(HttpRequestMessageProperty.Name, httpRequestProperty);
            }

            httpRequestProperty.Headers.Add(HttpRequestHeader.Cookie, cookies);
        }
#endif
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static CookieContainer ConstructCookieContainer(string serverUri, string username,
                                                                 string connectionString, string connectionStringProvider)
        {
            if (username == null)
            {
                if (Thread.CurrentPrincipal != null)
                    username = Thread.CurrentPrincipal.Identity.Name;
                else
                    username = string.Empty;
            }
            string [] allCookies = GetCookiesFromIECache(serverUri, username, connectionString, connectionStringProvider);
            if (allCookies == null || allCookies.Length < 1)
                return new CookieContainer();

            CookieContainer cc = new CookieContainer(allCookies.Length + 10, allCookies.Length + 10, 4096);
            Uri uri = new Uri(serverUri);
            for (int iter = 0; iter < allCookies.Length; iter++) {
                if (string.IsNullOrEmpty(allCookies[iter]))
                    continue;
                string name, value;
                int posEquals = allCookies[iter].IndexOf('=');
                if (posEquals < 0)
                {
                    name = allCookies[iter];
                    value = string.Empty;
                }
                else
                {
                    name = allCookies[iter].Substring(0, posEquals);
                    value = allCookies[iter].Substring(posEquals + 1);
                }
                name = name.Trim();
                value = value.Trim();
                if (name.Length == 32 && value == "Q") // this is our munged cookie. Skip it.
                    continue;
                cc.Add(new Cookie(name, value, "/", uri.Host));
            }
            return cc;
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        internal static bool DoAnyCookiesExist(string serverUri, string username, string connectionString, string connectionStringProvider)
        {
            string [] allCookies = GetCookiesFromIECache(serverUri, username, connectionString, connectionStringProvider);
            if (allCookies == null || allCookies.Length < 1)
                return false;
            foreach(string s in allCookies)
                if (s != null && s.Trim().Length > 0)
                    return true;
            return false;
        }
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults",
            MessageId = "System.Web.ClientServices.Providers.UnsafeNativeMethods.InternetSetCookieW(System.String,System.String,System.String)",
            Justification = "Failures of InternetSetCookieW are to be treated as benign.")]
        [SecuritySafeCritical]
        private static void StoreCookie(string serverUri, string cookieHeaders, string username, string connectionString, string connectionStringProvider)
        {
            if (string.IsNullOrEmpty(cookieHeaders))
                return;
            string[] cookieHeaderSplits = cookieHeaders.Split(new char[] { ',' });
            for(int iter=0; iter<cookieHeaderSplits.Length; )
            {
                StringBuilder cookieHeaderStrBuilder = new StringBuilder(cookieHeaderSplits[iter++]);
                while (iter < cookieHeaderSplits.Length)
                {
                    int posEq = cookieHeaderSplits[iter].IndexOf('=');
                    int posSemi = cookieHeaderSplits[iter].IndexOf(';');
                    if (posEq > 0 && (posSemi < 0 || posSemi > posEq))
                        break;

                    cookieHeaderStrBuilder.Append(",");
                    cookieHeaderStrBuilder.Append(cookieHeaderSplits[iter++]);
                }
                string cookieHeader = cookieHeaderStrBuilder.ToString();
                // Split it into name=value
                //Console.WriteLine("Saving cookie header:: " + cookieHeader);
                int posEquals = cookieHeader.IndexOf('=');
                string cookieName = ((posEquals < 0) ? cookieHeader : cookieHeader.Substring(0, posEquals)).Trim();
                string cookieValue = ((posEquals < 0) ? string.Empty : cookieHeader.Substring(posEquals + 1)).Trim();

                // trim off the HttpOnly and store in our DB
                if (cookieValue.Length > 0)
                    ChangeCookieAndStoreInDB(ref cookieName, ref cookieValue, username, connectionString, connectionStringProvider);
                if (UnsafeNativeMethods.InternetSetCookieW(serverUri, null, cookieName + " = " + cookieValue) == 0) {

                    // Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        [SecuritySafeCritical]
        private static string[] GetCookiesFromIECache(string uri, string username, string connectionString, string connectionStringProvider)
        {
            ////////////////////////////////////////////////////////////
            // Step 1: Get the cookie from IE
            int size = 0;
            if (UnsafeNativeMethods.InternetGetCookieW(uri, null, null, ref size) == 0 || size < 1)
                return null; // Failed to get cookie-size: likely, that no cookie is present
            StringBuilder cookieValue = new StringBuilder(size);
            if (UnsafeNativeMethods.InternetGetCookieW(uri, null, cookieValue, ref size) == 0)
                return null; // fail silently
            string [] cookies = cookieValue.ToString().Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            if (connectionString != null) {
                for (int iter = 0; iter < cookies.Length; iter++) {
                    cookies[iter] = GetCookieFromDB(cookies[iter], username, connectionString, connectionStringProvider);
                }
            }
            return cookies;
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static string GetCookieFromDB(string cookieHeader, string username, string connectionString, string connectionStringProvider)
        {
            cookieHeader = cookieHeader.Trim();

            /////////////////////////////////////////////////////////////////////////////////
            // Munged cookie is of the form "[32-digit-guid]=Q"
            // See if this cookie is of that form
            if (cookieHeader.Length != 34 || cookieHeader[33] != 'Q' || cookieHeader.IndexOf('=') != 32)
                return cookieHeader; // not of the correct form

            return SqlHelper.GetCookieFromDB(cookieHeader.Substring(0, 32), username, connectionString, connectionStringProvider);
        }

        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        private static void ChangeCookieAndStoreInDB(ref string cookieName, ref string cookieValue, string username, string connectionString, string connectionStringProvider)
        {
            string [] cookieProps = cookieValue.Split(new char[] {';'});
            if (cookieProps.Length < 1)
                return;

            string          actualCookieValue   =   cookieProps[0];
            bool            foundHttpOnly       =   false;
            StringBuilder   sb                  =   new StringBuilder((connectionString==null) ? actualCookieValue : "Q", cookieValue.Length);

            // Deal with all the properties, e.g. "path=/; expires= NNNN"
            for(int iter=1; iter<cookieProps.Length; iter++) {
                if (string.Compare(cookieProps[iter].Trim(), "HttpOnly", StringComparison.OrdinalIgnoreCase) == 0) {
                    foundHttpOnly = true;
                } else {
                    sb.Append(";" + cookieProps[iter]);
                }
            }
            if (!foundHttpOnly)
                return; // do nothing if this is NOT a http-only cookie

            if (connectionString != null) {
                string newCookieName = SqlHelper.StoreCookieInDB(cookieName, actualCookieValue, username, connectionString, connectionStringProvider);
                if (string.IsNullOrEmpty(newCookieName)) // do nothing on failure
                    return;
                cookieName = newCookieName;
            }

            cookieName = cookieName.Trim();
            if (actualCookieValue.Length < 1) // No value: browser wants us to delete the cookie
                cookieValue = ";" + sb.ToString().Substring((connectionString==null) ? 0 : 1);
            else
                cookieValue = sb.ToString().Trim();
            // System.Windows.Forms.MessageBox.Show("In ChangeCookieAndStoreInDB cookieValue=" + cookieValue + " actualCookieValue=" + actualCookieValue + " cookieName=" + cookieName);
        }
    }
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    internal static class UnsafeNativeMethods
    {
        // [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int InternetSetCookieW(string uri, string cookieName, string cookieValue);

        // [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int InternetGetCookieW(string uri, string cookieName, StringBuilder cookieValue, ref int dwSize);
    }
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////

#if ENABLE_WCF_SUPPORT
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0"),
     System.ServiceModel.ServiceContractAttribute(ConfigurationName = "LoginService")]
    internal interface LoginService
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/LoginService/Login", ReplyAction = "http://tempuri.org/LoginService/LoginResponse")]
        bool Login(string username, string password, string customCredential, bool isPersistent);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/LoginService/IsLoggedIn", ReplyAction = "http://tempuri.org/LoginService/IsLoggedInResponse")]
        bool IsLoggedIn();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/LoginService/Logout", ReplyAction = "http://tempuri.org/LoginService/LogoutResponse")]
        void Logout();
    }

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    internal interface LoginServiceChannel : LoginService, System.ServiceModel.IClientChannel
    {
    }

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "RolesService")]
    internal interface RolesService
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/RolesService/GetRolesForCurrentUser", ReplyAction = "http://tempuri.org/RolesService/GetRolesForCurrentUserResponse")]
        string[] GetRolesForCurrentUser();
    }

    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    internal interface RolesServiceChannel : RolesService, System.ServiceModel.IClientChannel
    {
    }



    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "ProfileService")]
    [ServiceKnownType("GetKnownTypes", typeof(ClientSettingsProvider))]
    internal interface ProfileService
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ProfileService/GetPropertiesForCurrentUser", ReplyAction = "http://tempuri.org/ProfileService/GetPropertiesForCurrentUserResponse"),

         SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        //[NetDataContractFormat]
        System.Collections.Generic.Dictionary<string, object> GetPropertiesForCurrentUser(string[] properties, bool authenticatedUserOnly);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ProfileService/SetPropertiesForCurrentUser", ReplyAction = "http://tempuri.org/ProfileService/SetPropertiesForCurrentUserResponse"),

         SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        //[NetDataContractFormat]
        Collection<string> SetPropertiesForCurrentUser(System.Collections.Generic.IDictionary<string, object> values, bool authenticatedUserOnly);

        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ProfileService/GetPropertiesMetadata", ReplyAction="http://tempuri.org/ProfileService/GetPropertiesMetadataResponse"),
         SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        System.Web.ApplicationServices.ProfilePropertyMetadata[] GetPropertiesMetadata();
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    internal interface ProfileServiceChannel : ProfileService, System.ServiceModel.IClientChannel
    {
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    internal partial class ProfileServiceClient : System.ServiceModel.ClientBase<ProfileService>, ProfileService
    {

        public ProfileServiceClient()
        {
        }

        public ProfileServiceClient(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        public ProfileServiceClient(string endpointConfigurationName, string remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        public ProfileServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        public ProfileServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }


        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        public System.Collections.Generic.Dictionary<string, object> GetPropertiesForCurrentUser(string[] propertyNames, bool authenticatedUserOnly)
        {
            return base.Channel.GetPropertiesForCurrentUser(propertyNames, authenticatedUserOnly);
        }

        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        public Collection<string> SetPropertiesForCurrentUser(System.Collections.Generic.IDictionary<string, object> values, bool authenticatedUserOnly)
        {
            return base.Channel.SetPropertiesForCurrentUser(values, authenticatedUserOnly);
        }

        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
       public System.Web.ApplicationServices.ProfilePropertyMetadata[] GetPropertiesMetadata()
       {
           return base.Channel.GetPropertiesMetadata();
       }
    }
#endif
}


