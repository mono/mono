using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Diagnostics;

namespace System.Net
{
    internal class ServiceNameStore
    {
        private List<string> serviceNames;
        private ServiceNameCollection serviceNameCollection;

        public ServiceNameCollection ServiceNames
        {
            get {                
                if (serviceNameCollection == null) {
                    serviceNameCollection = new ServiceNameCollection(serviceNames);                
                }                
                return serviceNameCollection;
            }
        }

        public ServiceNameStore()
        {
            serviceNames = new List<string>();
            serviceNameCollection = null; // set only when needed (due to expensive item-by-item copy)
        }

        private bool AddSingleServiceName(string spn)
        {
            spn = ServiceNameCollection.NormalizeServiceName(spn);
            if (Contains(spn)) 
            {
                return false;
            }
            else 
            {
                serviceNames.Add(spn);
                return true;
            }
        }

        public bool Add(string uriPrefix)
        {
            Debug.Assert(!String.IsNullOrEmpty(uriPrefix));

            string[] newServiceNames = BuildServiceNames(uriPrefix);
            
            bool addedAny = false;
            foreach (string spn in newServiceNames) 
            {
                if (AddSingleServiceName(spn)) 
                {
                    addedAny = true;

                    if (Logging.On) 
                    {
                        Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" +
                            ValidationHelper.HashString(this) + "::Add() " 
                            + SR.GetString(SR.net_log_listener_spn_add, spn, uriPrefix));
                    }
                }
            }
            
            if (addedAny) 
            {
                serviceNameCollection = null;
            }
            else if (Logging.On)
            {
                Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" +
                    ValidationHelper.HashString(this) + "::Add() " 
                    + SR.GetString(SR.net_log_listener_spn_not_add, uriPrefix));
            }

            return addedAny;
        }

        public bool Remove(string uriPrefix)
        {
            Debug.Assert(!String.IsNullOrEmpty(uriPrefix));

            string newServiceName = BuildSimpleServiceName(uriPrefix);
            newServiceName = ServiceNameCollection.NormalizeServiceName(newServiceName);
            bool needToRemove = Contains(newServiceName);

            if (needToRemove) {
                serviceNames.Remove(newServiceName);
                serviceNameCollection = null; //invalidate (readonly) ServiceNameCollection
            }

            if (Logging.On) {
                if (needToRemove) {
                    Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" +
                        ValidationHelper.HashString(this) + "::Remove() " 
                        + SR.GetString(SR.net_log_listener_spn_remove, newServiceName, uriPrefix));
                }
                else {
                    Logging.PrintInfo(Logging.HttpListener, "ServiceNameStore#" +
                        ValidationHelper.HashString(this) + "::Remove() " 
                        + SR.GetString(SR.net_log_listener_spn_not_remove, uriPrefix));
                }
            }

            return needToRemove;
        }

        // Assumes already normalized
        private bool Contains(string newServiceName)
        {
            if (newServiceName == null) {
                return false;
            }

            return ServiceNameCollection.Contains(newServiceName, serviceNames);
        }

        public void Clear()
        {
            serviceNames.Clear();
            serviceNameCollection = null; //invalidate (readonly) ServiceNameCollection
        }

        private string ExtractHostname(string uriPrefix, bool allowInvalidUriStrings)
        {
            if (Uri.IsWellFormedUriString(uriPrefix, UriKind.Absolute))
            {
                Uri hostUri = new Uri(uriPrefix);
                return hostUri.Host;
            }
            else if (allowInvalidUriStrings)
            {
                int i = uriPrefix.IndexOf("://") + 3;
                int j = i;

                bool inSquareBrackets = false;
                while(j < uriPrefix.Length && uriPrefix[j] != '/' && (uriPrefix[j] != ':' || inSquareBrackets)) 
                {
                    if (uriPrefix[j] == '[') 
                    {
                        if (inSquareBrackets) 
                        {
                            j = i;
                            break;
                        }
                        inSquareBrackets = true;
                    }
                    if (inSquareBrackets && uriPrefix[j] == ']') 
                    {
                        inSquareBrackets = false;
                    }
                    j++;
                }

                return uriPrefix.Substring(i, j - i);
            }

            return null;
        }

        public string BuildSimpleServiceName(string uriPrefix)
        {
            string hostname = ExtractHostname(uriPrefix, false);

            if (hostname != null)
            {
                return "HTTP/" + hostname;
            }
            else
            {
                return null;
            }
        }

        public string[] BuildServiceNames(string uriPrefix)
        {
            string hostname = ExtractHostname(uriPrefix, true);

            IPAddress ipAddress = null;
            if (String.Compare(hostname, "*", StringComparison.InvariantCultureIgnoreCase) == 0 || 
                String.Compare(hostname, "+", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                IPAddress.TryParse(hostname, out ipAddress)) 
            {
                // for a wildcard, register the machine name.  If the caller doesn't have DNS permission
                // or the query fails for some reason, don't add an SPN.
                try
                {
                    string machineName = Dns.GetHostEntry(String.Empty).HostName;
                    return new string[] { "HTTP/" + machineName };
                }
                catch (System.Net.Sockets.SocketException)
                {
                    return new string[0];
                }
                catch (System.Security.SecurityException)
                {
                    return new string[0];
                }
            }
            else if (!hostname.Contains("."))
            {
                // for a dotless name, try to resolve the FQDN.  If the caller doesn't have DNS permission
                // or the query fails for some reason, add only the dotless name.
                try
                {
                    string fqdn = Dns.GetHostEntry(hostname).HostName;
                    return new string[] { "HTTP/" + hostname, "HTTP/" + fqdn };
                }
                catch (System.Net.Sockets.SocketException)
                {
                    return new string[] { "HTTP/" + hostname };
                }
                catch (System.Security.SecurityException)
                {
                    return new string[] { "HTTP/" + hostname };
                }
            }
            else
            {
                return new string[] { "HTTP/" + hostname };
            }
        }
    }
}
