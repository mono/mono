//
// System.Net.ServicePointManager
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2003-2010 Novell, Inc (http://www.novell.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if SECURITY_DEP

#if MONOTOUCH || MONODROID
using Mono.Security.Protocol.Tls;
using MSX = Mono.Security.X509;
using Mono.Security.X509.Extensions;
#else
extern alias MonoSecurity;
using MonoSecurity::Mono.Security.X509.Extensions;
using MonoSecurity::Mono.Security.Protocol.Tls;
using MSX = MonoSecurity::Mono.Security.X509;
#endif

using System.Text.RegularExpressions;
#endif

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Configuration;
using System.Security.Cryptography.X509Certificates;

using System.Globalization;
using System.Net.Security;
using System.Diagnostics;

//
// notes:
// A service point manager manages service points (duh!).
// A service point maintains a list of connections (per scheme + authority).
// According to HttpWebRequest.ConnectionGroupName each connection group
// creates additional connections. therefor, a service point has a hashtable
// of connection groups where each value is a list of connections.
// 
// when we need to make an HttpWebRequest, we need to do the following:
// 1. find service point, given Uri and Proxy 
// 2. find connection group, given service point and group name
// 3. find free connection in connection group, or create one (if ok due to limits)
// 4. lease connection
// 5. execute request
// 6. when finished, return connection
//


namespace System.Net 
{
	public partial class ServicePointManager {
		class SPKey {
			Uri uri; // schema/host/port
			Uri proxy;
			bool use_connect;

			public SPKey (Uri uri, Uri proxy, bool use_connect) {
				this.uri = uri;
				this.proxy = proxy;
				this.use_connect = use_connect;
			}

			public Uri Uri {
				get { return uri; }
			}

			public bool UseConnect {
				get { return use_connect; }
			}

			public bool UsesProxy {
				get { return proxy != null; }
			}

			public override int GetHashCode () {
				int hash = 23;
				hash = hash * 31 + ((use_connect) ? 1 : 0);
				hash = hash * 31 + uri.GetHashCode ();
				hash = hash * 31 + (proxy != null ? proxy.GetHashCode () : 0);
				return hash;
			}

			public override bool Equals (object obj) {
				SPKey other = obj as SPKey;
				if (obj == null) {
					return false;
				}

				if (!uri.Equals (other.uri))
					return false;
				if (use_connect != other.use_connect || UsesProxy != other.UsesProxy)
					return false;
				if (UsesProxy && !proxy.Equals (other.proxy))
					return false;
				return true;
			}
		}

		private static HybridDictionary servicePoints = new HybridDictionary ();
		
		// Static properties
		
		private static ICertificatePolicy policy = new DefaultCertificatePolicy ();
		private static int defaultConnectionLimit = DefaultPersistentConnectionLimit;
		private static int maxServicePointIdleTime = 100000; // 100 seconds
		private static int maxServicePoints = 0;
		private static int dnsRefreshTimeout = 2 * 60 * 1000;
		private static bool _checkCRL = false;
		private static SecurityProtocolType _securityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

		static bool expectContinue = true;
		static bool useNagle;
		static ServerCertValidationCallback server_cert_cb;
		static bool tcp_keepalive;
		static int tcp_keepalive_time;
		static int tcp_keepalive_interval;

		// Fields
		
		public const int DefaultNonPersistentConnectionLimit = 4;
#if MONOTOUCH
		public const int DefaultPersistentConnectionLimit = 10;
#else
		public const int DefaultPersistentConnectionLimit = 2;
#endif

#if !NET_2_1
		const string configKey = "system.net/connectionManagement";
		static ConnectionManagementData manager;
#endif
		
		static ServicePointManager ()
		{
#if !NET_2_1
#if CONFIGURATION_DEP
			object cfg = ConfigurationManager.GetSection (configKey);
			ConnectionManagementSection s = cfg as ConnectionManagementSection;
			if (s != null) {
				manager = new ConnectionManagementData (null);
				foreach (ConnectionManagementElement e in s.ConnectionManagement)
					manager.Add (e.Address, e.MaxConnection);

				defaultConnectionLimit = (int) manager.GetMaxConnections ("*");				
				return;
			}
#endif
			manager = (ConnectionManagementData) ConfigurationSettings.GetConfig (configKey);
			if (manager != null) {
				defaultConnectionLimit = (int) manager.GetMaxConnections ("*");				
			}
#endif
		}

		// Constructors
		private ServicePointManager ()
		{
		}		
		
		// Properties
		
		[Obsolete ("Use ServerCertificateValidationCallback instead", false)]
		public static ICertificatePolicy CertificatePolicy {
			get { return policy; }
			set { policy = value; }
		}

		internal static ICertificatePolicy GetLegacyCertificatePolicy ()
		{
			return policy;
		}

		[MonoTODO("CRL checks not implemented")]
		public static bool CheckCertificateRevocationList {
			get { return _checkCRL; }
			set { _checkCRL = false; }	// TODO - don't yet accept true
		}
		
		public static int DefaultConnectionLimit {
			get { return defaultConnectionLimit; }
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				defaultConnectionLimit = value; 
#if !NET_2_1
                if (manager != null)
					manager.Add ("*", defaultConnectionLimit);
#endif
			}
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		public static int DnsRefreshTimeout
		{
			get {
				return dnsRefreshTimeout;
			}
			set {
				dnsRefreshTimeout = Math.Max (-1, value);
			}
		}
		
		[MonoTODO]
		public static bool EnableDnsRoundRobin
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
		
		public static int MaxServicePointIdleTime {
			get { 
				return maxServicePointIdleTime;
			}
			set { 
				if (value < -2 || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ("value");
				maxServicePointIdleTime = value;
			}
		}
		
		public static int MaxServicePoints {
			get { 
				return maxServicePoints; 
			}
			set {  
				if (value < 0)
					throw new ArgumentException ("value");				

				maxServicePoints = value;
			}
		}

		public static SecurityProtocolType SecurityProtocol {
			get { return _securityProtocol; }
			set { _securityProtocol = value; }
		}

		internal static ServerCertValidationCallback ServerCertValidationCallback {
			get { return server_cert_cb; }
		}

		public static RemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get {
				if (server_cert_cb == null)
					return null;
				return server_cert_cb.ValidationCallback;
			}
			set
			{
				if (value == null)
					server_cert_cb = null;
				else
					server_cert_cb = new ServerCertValidationCallback (value);
			}
		}

		public static bool Expect100Continue {
			get { return expectContinue; }
			set { expectContinue = value; }
		}

		public static bool UseNagleAlgorithm {
			get { return useNagle; }
			set { useNagle = value; }
		}

		internal static bool DisableStrongCrypto {
			get { return false; }
		}

		// Methods
		public static void SetTcpKeepAlive (bool enabled, int keepAliveTime, int keepAliveInterval)
		{
			if (enabled) {
				if (keepAliveTime <= 0)
					throw new ArgumentOutOfRangeException ("keepAliveTime", "Must be greater than 0");
				if (keepAliveInterval <= 0)
					throw new ArgumentOutOfRangeException ("keepAliveInterval", "Must be greater than 0");
			}

			tcp_keepalive = enabled;
			tcp_keepalive_time = keepAliveTime;
			tcp_keepalive_interval = keepAliveInterval;
		}

		public static ServicePoint FindServicePoint (Uri address) 
		{
			return FindServicePoint (address, GlobalProxySelection.Select);
		}
		
		public static ServicePoint FindServicePoint (string uriString, IWebProxy proxy)
		{
			return FindServicePoint (new Uri(uriString), proxy);
		}

		public static ServicePoint FindServicePoint (Uri address, IWebProxy proxy)
		{
			if (address == null)
				throw new ArgumentNullException ("address");

			var origAddress = new Uri (address.Scheme + "://" + address.Authority);
			
			bool usesProxy = false;
			bool useConnect = false;
			if (proxy != null && !proxy.IsBypassed(address)) {
				usesProxy = true;
				bool isSecure = address.Scheme == "https";
				address = proxy.GetProxy (address);
				if (address.Scheme != "http" && !isSecure)
					throw new NotSupportedException ("Proxy scheme not supported.");

				if (isSecure && address.Scheme == "http")
					useConnect = true;
			} 

			address = new Uri (address.Scheme + "://" + address.Authority);
			
			ServicePoint sp = null;
			SPKey key = new SPKey (origAddress, usesProxy ? address : null, useConnect);
			lock (servicePoints) {
				sp = servicePoints [key] as ServicePoint;
				if (sp != null)
					return sp;

				if (maxServicePoints > 0 && servicePoints.Count >= maxServicePoints)
					throw new InvalidOperationException ("maximum number of service points reached");

				int limit;
#if NET_2_1
				limit = defaultConnectionLimit;
#else
				string addr = address.ToString ();
				limit = (int) manager.GetMaxConnections (addr);
#endif
				sp = new ServicePoint (address, limit, maxServicePointIdleTime);
				sp.Expect100Continue = expectContinue;
				sp.UseNagleAlgorithm = useNagle;
				sp.UsesProxy = usesProxy;
				sp.UseConnect = useConnect;
				sp.SetTcpKeepAlive (tcp_keepalive, tcp_keepalive_time, tcp_keepalive_interval);
				servicePoints.Add (key, sp);
			}
			
			return sp;
		}

		internal static void CloseConnectionGroup (string connectionGroupName)
		{
			lock (servicePoints) {
				foreach (ServicePoint sp in servicePoints.Values) {
					sp.CloseConnectionGroup (connectionGroupName);
				}
			}
		}
		
#if SECURITY_DEP
		internal class ChainValidationHelper {
			object sender;
			string host;
			RemoteCertificateValidationCallback cb;

#if !MONOTOUCH
			static bool is_macosx = System.IO.File.Exists (OSX509Certificates.SecurityLibrary);
			static X509RevocationMode revocation_mode;

			static ChainValidationHelper ()
			{
				revocation_mode = X509RevocationMode.NoCheck;
				try {
					string str = Environment.GetEnvironmentVariable ("MONO_X509_REVOCATION_MODE");
					if (String.IsNullOrEmpty (str))
						return;
					revocation_mode = (X509RevocationMode) Enum.Parse (typeof (X509RevocationMode), str, true);
				} catch {
				}
			}
#endif

			public ChainValidationHelper (object sender, string hostName)
			{
				this.sender = sender;
				host = hostName;
			}

			public RemoteCertificateValidationCallback ServerCertificateValidationCallback {
				get {
					if (cb == null)
						cb = ServicePointManager.ServerCertificateValidationCallback;
					return cb;
				}
				set { cb = value; }
			}

			// Used when the obsolete ICertificatePolicy is set to DefaultCertificatePolicy
			// and the new ServerCertificateValidationCallback is not null
			internal ValidationResult ValidateChain (MSX.X509CertificateCollection certs)
			{
				// user_denied is true if the user callback is called and returns false
				bool user_denied = false;
				if (certs == null || certs.Count == 0)
					return null;

				ICertificatePolicy policy = ServicePointManager.CertificatePolicy;

				X509Certificate2 leaf = new X509Certificate2 (certs [0].RawData);
				int status11 = 0; // Error code passed to the obsolete ICertificatePolicy callback
				SslPolicyErrors errors = 0;
				X509Chain chain = null;
				bool result = false;
#if MONOTOUCH
				// The X509Chain is not really usable with MonoTouch (since the decision is not based on this data)
				// However if someone wants to override the results (good or bad) from iOS then they will want all
				// the certificates that the server provided (which generally does not include the root) so, only  
				// if there's a user callback, we'll create the X509Chain but won't build it
				// ref: https://bugzilla.xamarin.com/show_bug.cgi?id=7245
				if (ServerCertificateValidationCallback != null) {
#endif
				chain = new X509Chain ();
				chain.ChainPolicy = new X509ChainPolicy ();
#if !MONOTOUCH
				chain.ChainPolicy.RevocationMode = revocation_mode;
#endif
				for (int i = 1; i < certs.Count; i++) {
					X509Certificate2 c2 = new X509Certificate2 (certs [i].RawData);
					chain.ChainPolicy.ExtraStore.Add (c2);
				}
#if MONOTOUCH
				}
#else
				try {
					if (!chain.Build (leaf))
						errors |= GetErrorsFromChain (chain);
				} catch (Exception e) {
					Console.Error.WriteLine ("ERROR building certificate chain: {0}", e);
					Console.Error.WriteLine ("Please, report this problem to the Mono team");
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
				}

				// for OSX and iOS we're using the native API to check for the SSL server policy and host names
				if (!is_macosx) {
					if (!CheckCertificateUsage (leaf)) {
						errors |= SslPolicyErrors.RemoteCertificateChainErrors;
						status11 = -2146762490; //CERT_E_PURPOSE 0x800B0106
					}

					if (!CheckServerIdentity (certs [0], host)) {
						errors |= SslPolicyErrors.RemoteCertificateNameMismatch;
						status11 = -2146762481; // CERT_E_CN_NO_MATCH 0x800B010F
					}
				} else {
#endif
					// Attempt to use OSX certificates
					// Ideally we should return the SecTrustResult
					OSX509Certificates.SecTrustResult trustResult = OSX509Certificates.SecTrustResult.Deny;
					try {
						trustResult = OSX509Certificates.TrustEvaluateSsl (certs, host);
						// We could use the other values of trustResult to pass this extra information
						// to the .NET 2 callback for values like SecTrustResult.Confirm
						result = (trustResult == OSX509Certificates.SecTrustResult.Proceed ||
								  trustResult == OSX509Certificates.SecTrustResult.Unspecified);
					} catch {
						// Ignore
					}
					
					if (result) {
						// TrustEvaluateSsl was successful so there's no trust error
						// IOW we discard our own chain (since we trust OSX one instead)
						errors = 0;
					} else {
						// callback and DefaultCertificatePolicy needs this since 'result' is not specified
						status11 = (int) trustResult;
						errors |= SslPolicyErrors.RemoteCertificateChainErrors;
					}
#if !MONOTOUCH
				}
#endif

#if MONODROID && SECURITY_DEP
				result = AndroidPlatform.TrustEvaluateSsl (certs, sender, leaf, chain, errors);
				if (result) {
					// chain.Build() + GetErrorsFromChain() (above) will ALWAYS fail on
					// Android (there are no mozroots or preinstalled root certificates),
					// thus `errors` will ALWAYS have RemoteCertificateChainErrors.
					// Android just verified the chain; clear RemoteCertificateChainErrors.
					errors  &= ~SslPolicyErrors.RemoteCertificateChainErrors;
				}
#endif

				if (policy != null && (!(policy is DefaultCertificatePolicy) || cb == null)) {
					ServicePoint sp = null;
					HttpWebRequest req = sender as HttpWebRequest;
					if (req != null)
						sp = req.ServicePointNoLock;
					if (status11 == 0 && errors != 0)
						status11 = GetStatusFromChain (chain);

					// pre 2.0 callback
					result = policy.CheckValidationResult (sp, leaf, req, status11);
					user_denied = !result && !(policy is DefaultCertificatePolicy);
				}
				// If there's a 2.0 callback, it takes precedence
				if (ServerCertificateValidationCallback != null) {
					result = ServerCertificateValidationCallback (sender, leaf, chain, errors);
					user_denied = !result;
				}
				return new ValidationResult (result, user_denied, status11);
			}

			static int GetStatusFromChain (X509Chain chain)
			{
				long result = 0;
				foreach (var status in chain.ChainStatus) {
					X509ChainStatusFlags flags = status.Status;
					if (flags == X509ChainStatusFlags.NoError)
						continue;

					// CERT_E_EXPIRED
					if ((flags & X509ChainStatusFlags.NotTimeValid) != 0) result = 0x800B0101;
					// CERT_E_VALIDITYPERIODNESTING
					else if ((flags & X509ChainStatusFlags.NotTimeNested) != 0) result = 0x800B0102;
					// CERT_E_REVOKED
					else if ((flags & X509ChainStatusFlags.Revoked) != 0) result = 0x800B010C;
					// TRUST_E_CERT_SIGNATURE
					else if ((flags & X509ChainStatusFlags.NotSignatureValid) != 0) result = 0x80096004;
					// CERT_E_WRONG_USAGE
					else if ((flags & X509ChainStatusFlags.NotValidForUsage) != 0) result = 0x800B0110;
					// CERT_E_UNTRUSTEDROOT
					else if ((flags & X509ChainStatusFlags.UntrustedRoot) != 0) result = 0x800B0109;
					// CRYPT_E_NO_REVOCATION_CHECK
					else if ((flags & X509ChainStatusFlags.RevocationStatusUnknown) != 0) result = 0x80092012;
					// CERT_E_CHAINING
					else if ((flags & X509ChainStatusFlags.Cyclic) != 0) result = 0x800B010A;
					// TRUST_E_FAIL - generic
					else if ((flags & X509ChainStatusFlags.InvalidExtension) != 0) result = 0x800B010B;
					// CERT_E_UNTRUSTEDROOT
					else if ((flags & X509ChainStatusFlags.InvalidPolicyConstraints) != 0) result = 0x800B010D;
					// TRUST_E_BASIC_CONSTRAINTS
					else if ((flags & X509ChainStatusFlags.InvalidBasicConstraints) != 0) result = 0x80096019;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.InvalidNameConstraints) != 0) result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasNotSupportedNameConstraint) != 0) result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasNotDefinedNameConstraint) != 0) result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasNotPermittedNameConstraint) != 0) result = 0x800B0114;
					// CERT_E_INVALID_NAME
					else if ((flags & X509ChainStatusFlags.HasExcludedNameConstraint) != 0) result = 0x800B0114;
					// CERT_E_CHAINING
					else if ((flags & X509ChainStatusFlags.PartialChain) != 0) result = 0x800B010A;
					// CERT_E_EXPIRED
					else if ((flags & X509ChainStatusFlags.CtlNotTimeValid) != 0) result = 0x800B0101;
					// TRUST_E_CERT_SIGNATURE
					else if ((flags & X509ChainStatusFlags.CtlNotSignatureValid) != 0) result = 0x80096004;
					// CERT_E_WRONG_USAGE
					else if ((flags & X509ChainStatusFlags.CtlNotValidForUsage) != 0) result = 0x800B0110;
					// CRYPT_E_NO_REVOCATION_CHECK
					else if ((flags & X509ChainStatusFlags.OfflineRevocation) != 0) result = 0x80092012;
					// CERT_E_ISSUERCHAINING
					else if ((flags & X509ChainStatusFlags.NoIssuanceChainPolicy) != 0) result = 0x800B0107;
					else result = 0x800B010B; // TRUST_E_FAIL - generic

					break; // Exit the loop on the first error
				}
				return (int) result;
			}
#if !MONOTOUCH
			static SslPolicyErrors GetErrorsFromChain (X509Chain chain)
			{
				SslPolicyErrors errors = SslPolicyErrors.None;
				foreach (var status in chain.ChainStatus) {
					if (status.Status == X509ChainStatusFlags.NoError)
						continue;
					errors |= SslPolicyErrors.RemoteCertificateChainErrors;
					break;
				}
				return errors;
			}

			static X509KeyUsageFlags s_flags = X509KeyUsageFlags.DigitalSignature  | 
							X509KeyUsageFlags.KeyAgreement |
							X509KeyUsageFlags.KeyEncipherment;
			// Adapted to System 2.0+ from TlsServerCertificate.cs
			//------------------------------
			// Note: this method only works for RSA certificates
			// DH certificates requires some changes - does anyone use one ?
			static bool CheckCertificateUsage (X509Certificate2 cert) 
			{
				try {
					// certificate extensions are required for this
					// we "must" accept older certificates without proofs
					if (cert.Version < 3)
						return true;

					X509KeyUsageExtension kux = (cert.Extensions ["2.5.29.15"] as X509KeyUsageExtension);
					X509EnhancedKeyUsageExtension eku = (cert.Extensions ["2.5.29.37"] as X509EnhancedKeyUsageExtension);
					if (kux != null && eku != null) {
						// RFC3280 states that when both KeyUsageExtension and 
						// ExtendedKeyUsageExtension are present then BOTH should
						// be valid
						if ((kux.KeyUsages & s_flags) == 0)
							return false;
						return eku.EnhancedKeyUsages ["1.3.6.1.5.5.7.3.1"] != null ||
							eku.EnhancedKeyUsages ["2.16.840.1.113730.4.1"] != null;
					} else if (kux != null) {
						return ((kux.KeyUsages & s_flags) != 0);
					} else if (eku != null) {
						// Server Authentication (1.3.6.1.5.5.7.3.1) or
						// Netscape Server Gated Crypto (2.16.840.1.113730.4)
						return eku.EnhancedKeyUsages ["1.3.6.1.5.5.7.3.1"] != null ||
							eku.EnhancedKeyUsages ["2.16.840.1.113730.4.1"] != null;
					}

					// last chance - try with older (deprecated) Netscape extensions
					X509Extension ext = cert.Extensions ["2.16.840.1.113730.1.1"];
					if (ext != null) {
						string text = ext.NetscapeCertType (false);
						return text.IndexOf ("SSL Server Authentication", StringComparison.Ordinal) != -1;
					}
					return true;
				} catch (Exception e) {
					Console.Error.WriteLine ("ERROR processing certificate: {0}", e);
					Console.Error.WriteLine ("Please, report this problem to the Mono team");
					return false;
				}
			}

			// RFC2818 - HTTP Over TLS, Section 3.1
			// http://www.ietf.org/rfc/rfc2818.txt
			// 
			// 1.	if present MUST use subjectAltName dNSName as identity
			// 1.1.		if multiples entries a match of any one is acceptable
			// 1.2.		wildcard * is acceptable
			// 2.	URI may be an IP address -> subjectAltName.iPAddress
			// 2.1.		exact match is required
			// 3.	Use of the most specific Common Name (CN=) in the Subject
			// 3.1		Existing practice but DEPRECATED
			static bool CheckServerIdentity (MSX.X509Certificate cert, string targetHost) 
			{
				try {
					MSX.X509Extension ext = cert.Extensions ["2.5.29.17"];
					// 1. subjectAltName
					if (ext != null) {
						SubjectAltNameExtension subjectAltName = new SubjectAltNameExtension (ext);
						// 1.1 - multiple dNSName
						foreach (string dns in subjectAltName.DNSNames) {
							// 1.2 TODO - wildcard support
							if (Match (targetHost, dns))
								return true;
						}
						// 2. ipAddress
						foreach (string ip in subjectAltName.IPAddresses) {
							// 2.1. Exact match required
							if (ip == targetHost)
								return true;
						}
					}
					// 3. Common Name (CN=)
					return CheckDomainName (cert.SubjectName, targetHost);
				} catch (Exception e) {
					Console.Error.WriteLine ("ERROR processing certificate: {0}", e);
					Console.Error.WriteLine ("Please, report this problem to the Mono team");
					return false;
				}
			}

			static bool CheckDomainName (string subjectName, string targetHost)
			{
				string	domainName = String.Empty;
				Regex search = new Regex(@"CN\s*=\s*([^,]*)");
				MatchCollection	elements = search.Matches(subjectName);
				if (elements.Count == 1) {
					if (elements[0].Success)
						domainName = elements[0].Groups[1].Value.ToString();
				}

				return Match (targetHost, domainName);
			}

			// ensure the pattern is valid wrt to RFC2595 and RFC2818
			// http://www.ietf.org/rfc/rfc2595.txt
			// http://www.ietf.org/rfc/rfc2818.txt
			static bool Match (string hostname, string pattern)
			{
				// check if this is a pattern
				int index = pattern.IndexOf ('*');
				if (index == -1) {
					// not a pattern, do a direct case-insensitive comparison
					return (String.Compare (hostname, pattern, true, CultureInfo.InvariantCulture) == 0);
				}

				// check pattern validity
				// A "*" wildcard character MAY be used as the left-most name component in the certificate.

				// unless this is the last char (valid)
				if (index != pattern.Length - 1) {
					// then the next char must be a dot .'.
					if (pattern [index + 1] != '.')
						return false;
				}

				// only one (A) wildcard is supported
				int i2 = pattern.IndexOf ('*', index + 1);
				if (i2 != -1)
					return false;

				// match the end of the pattern
				string end = pattern.Substring (index + 1);
				int length = hostname.Length - end.Length;
				// no point to check a pattern that is longer than the hostname
				if (length <= 0)
					return false;

				if (String.Compare (hostname, length, end, 0, end.Length, true, CultureInfo.InvariantCulture) != 0)
					return false;

				// special case, we start with the wildcard
				if (index == 0) {
					// ensure we hostname non-matched part (start) doesn't contain a dot
					int i3 = hostname.IndexOf ('.');
					return ((i3 == -1) || (i3 >= (hostname.Length - end.Length)));
				}

				// match the start of the pattern
				string start = pattern.Substring (0, index);
				return (String.Compare (hostname, 0, start, 0, start.Length, true, CultureInfo.InvariantCulture) == 0);
			}
#endif
		}
#endif
	}
}

