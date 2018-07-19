//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    [DataContract]
    sealed class BaseUriWithWildcard
    {
        [DataMember]
        Uri baseAddress;

        const char segmentDelimiter = '/';

        [DataMember]
        HostNameComparisonMode hostNameComparisonMode;
        const string plus = "+";
        const string star = "*";
        const int HttpUriDefaultPort = 80;
        const int HttpsUriDefaultPort = 443;

        // Derived from [DataMember] fields
        Comparand comparand;
        int hashCode;

        public BaseUriWithWildcard(Uri baseAddress, HostNameComparisonMode hostNameComparisonMode)
        {
            this.baseAddress = baseAddress;
            this.hostNameComparisonMode = hostNameComparisonMode;
            this.SetComparisonAddressAndHashCode();

            // Note the Uri may contain query string for WSDL purpose.
            // So do not check IsValid().
        }

        BaseUriWithWildcard(string protocol, int defaultPort, string binding, int segmentCount, string path, string sampleBinding)
        {
            string[] urlParameters = SplitBinding(binding);

            if (urlParameters.Length != segmentCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new UriFormatException(SR.GetString(SR.Hosting_MisformattedBinding, binding, protocol, sampleBinding)));
            }

            int currentIndex = segmentCount - 1;
            string host = ParseHostAndHostNameComparisonMode(urlParameters[currentIndex]);

            int port = -1;

            if (--currentIndex >= 0)
            {
                string portString = urlParameters[currentIndex].Trim();

                if (!string.IsNullOrEmpty(portString) &&
                    !int.TryParse(portString, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out port))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(SR.GetString(SR.Hosting_MisformattedPort, protocol, binding, portString)));
                }

                if (port == defaultPort)
                {
                    // Set to -1 so that Uri does not show it in the string.
                    port = -1;
                }
            }
            try
            {
                Fx.Assert(path != null, "path should never be null here");
                this.baseAddress = new UriBuilder(protocol, host, port, path).Uri;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Error);

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(SR.GetString(SR.Hosting_MisformattedBindingData, binding,
                    protocol)));
            }
            SetComparisonAddressAndHashCode();
        }

        internal Uri BaseAddress
        {
            get { return this.baseAddress; }
        }

        internal HostNameComparisonMode HostNameComparisonMode
        {
            get { return this.hostNameComparisonMode; }
        }

        static string[] SplitBinding(string binding)
        {
            bool parsingIPv6Address = false;
            string[] tokens = null;
            const char splitChar = ':', startIPv6Address = '[', endIPv6Address = ']';

            List<int> splitLocations = null;

            for (int i = 0; i < binding.Length; i++)
            {
                if (parsingIPv6Address && binding[i] == endIPv6Address)
                {
                    parsingIPv6Address = false;
                }
                else if (binding[i] == startIPv6Address)
                {
                    parsingIPv6Address = true;
                }
                else if (!parsingIPv6Address && binding[i] == splitChar)
                {
                    if (splitLocations == null)
                    {
                        splitLocations = new List<int>();
                    }
                    splitLocations.Add(i);
                }
            }

            if (splitLocations == null)
            {
                tokens = new string[] { binding };
            }
            else
            {
                tokens = new string[splitLocations.Count + 1];
                int startIndex = 0;
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (i < splitLocations.Count)
                    {
                        int nextSplitIndex = splitLocations[i];
                        tokens[i] = binding.Substring(startIndex, nextSplitIndex - startIndex);
                        startIndex = nextSplitIndex + 1;
                    }
                    else //splitting the last segment
                    {
                        if (startIndex < binding.Length)
                        {
                            tokens[i] = binding.Substring(startIndex, binding.Length - startIndex);
                        }
                        else
                        {
                            //splitChar was the last character in the string
                            tokens[i] = string.Empty;
                        }
                    }
                }
            }
            return tokens;
        }

        internal static BaseUriWithWildcard CreateHostedUri(string protocol, string binding, string path)
        {
            Fx.Assert(protocol != null, "caller must verify");

            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }

            if (path == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("path");
            }

            if (protocol.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                // For http, binding format is: "<ipAddress>:<port>:<hostName>"
                // as specified in http://www.microsoft.com/resources/documentation/WindowsServ/2003/standard/proddocs/en-us/Default.asp?url=/resources/documentation/WindowsServ/2003/standard/proddocs/en-us/ref_mb_serverbindings.asp
                return new BaseUriWithWildcard(Uri.UriSchemeHttp, HttpUriDefaultPort, binding, 3, path, ":80:");
            }
            else if (protocol.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                // For https, binding format is the same as http
                return new BaseUriWithWildcard(Uri.UriSchemeHttps, HttpsUriDefaultPort, binding, 3, path, ":443:");
            }
            else if (protocol.Equals(Uri.UriSchemeNetTcp, StringComparison.OrdinalIgnoreCase))
            {
                // For net.tcp, binding format is: "<port>:<hostName>"
                return new BaseUriWithWildcard(Uri.UriSchemeNetTcp, TcpUri.DefaultPort, binding, 2, path, "808:*");
            }
            else if (protocol.Equals(Uri.UriSchemeNetPipe, StringComparison.OrdinalIgnoreCase))
            {
                return CreateHostedPipeUri(binding, path);
            }
            else if (protocol.Equals(MsmqUri.NetMsmqAddressTranslator.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseUriWithWildcard(MsmqUri.NetMsmqAddressTranslator.Scheme, -1, binding, 1, path, "*");
            }
            else if (protocol.Equals(MsmqUri.FormatNameAddressTranslator.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseUriWithWildcard(MsmqUri.FormatNameAddressTranslator.Scheme, -1, binding, 1, path, "*");
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new UriFormatException(SR.GetString(SR.Hosting_NotSupportedProtocol, binding)));
        }

        internal static BaseUriWithWildcard CreateHostedPipeUri(string binding, string path)
        {
            // For net.pipe, binding format is: "<hostName>"
            return new BaseUriWithWildcard(Uri.UriSchemeNetPipe, -1, binding, 1, path, "*");
        }

        public override bool Equals(object o)
        {
            BaseUriWithWildcard other = o as BaseUriWithWildcard;

            if (other == null || other.hashCode != this.hashCode || other.hostNameComparisonMode != this.hostNameComparisonMode ||
                other.comparand.Port != this.comparand.Port)
            {
                return false;
            }
            if (!object.ReferenceEquals(other.comparand.Scheme, this.comparand.Scheme))
            {
                return false;
            }
            return this.comparand.Address.Equals(other.comparand.Address);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        internal bool IsBaseOf(Uri fullAddress)
        {
            if ((object)baseAddress.Scheme != (object)fullAddress.Scheme)
            {
                return false;
            }

            if (baseAddress.Port != fullAddress.Port)
            {
                return false;
            }

            if (this.HostNameComparisonMode == HostNameComparisonMode.Exact)
            {
                if (string.Compare(baseAddress.Host, fullAddress.Host, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
            }
            string s1 = baseAddress.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);
            string s2 = fullAddress.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.Unescaped);

            if (s1.Length > s2.Length)
            {
                return false;
            }

            if (s1.Length < s2.Length &&
                s1[s1.Length - 1] != segmentDelimiter &&
                s2[s1.Length] != segmentDelimiter)
            {
                // Matching over segments
                return false;
            }
            return string.Compare(s2, 0, s1, 0, s1.Length, StringComparison.OrdinalIgnoreCase) == 0;
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            UriSchemeKeyedCollection.ValidateBaseAddress(baseAddress, "context");

            if (!HostNameComparisonModeHelper.IsDefined(this.HostNameComparisonMode))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("context", SR.GetString(SR.Hosting_BaseUriDeserializedNotValid));
            }
            this.SetComparisonAddressAndHashCode();
        }

        string ParseHostAndHostNameComparisonMode(string host)
        {
            if (string.IsNullOrEmpty(host) || host.Equals(star))
            {
                hostNameComparisonMode = HostNameComparisonMode.WeakWildcard;
                host = DnsCache.MachineName;
            }
            else if (host.Equals(plus))
            {
                hostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
                host = DnsCache.MachineName;
            }
            else
            {
                hostNameComparisonMode = HostNameComparisonMode.Exact;
            }
            return host;
        }

        void SetComparisonAddressAndHashCode()
        {
            if (this.HostNameComparisonMode == HostNameComparisonMode.Exact)
            {
                // Use canonical string representation of the full base address for comparison
                this.comparand.Address = this.baseAddress.ToString();
            }
            else
            {
                // Use canonical string representation of the absolute path for comparison
                this.comparand.Address = this.baseAddress.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
            }

            this.comparand.Port = this.baseAddress.Port;
            this.comparand.Scheme = this.baseAddress.Scheme;

            if ((this.comparand.Port == -1) && ((object)this.comparand.Scheme == (object)Uri.UriSchemeNetTcp))
            {
                // Compensate for the fact that the Uri type doesn't know about our default TCP port number
                this.comparand.Port = TcpUri.DefaultPort;
            }
            this.hashCode = this.comparand.Address.GetHashCode() ^ this.comparand.Port ^ (int)this.HostNameComparisonMode;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", this.HostNameComparisonMode, this.BaseAddress);
        }

        struct Comparand
        {
            public string Address;
            public int Port;
            public string Scheme;
        }
    }
}
