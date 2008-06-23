//
// System.Net.ServicePointManager
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Configuration;
using System.Security.Cryptography.X509Certificates;

#if NET_2_0
using System.Net.Security;
#endif

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
	public class ServicePointManager
	{
		private static HybridDictionary servicePoints = new HybridDictionary ();
		
		// Static properties
		
		private static ICertificatePolicy policy = new DefaultCertificatePolicy ();
		private static int defaultConnectionLimit = DefaultPersistentConnectionLimit;
		private static int maxServicePointIdleTime = 900000; // 15 minutes
		private static int maxServicePoints = 0;
		private static bool _checkCRL = false;
		private static SecurityProtocolType _securityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

#if NET_1_1
#if TARGET_JVM
		static bool expectContinue = false;
#else
		static bool expectContinue = true;
#endif
		static bool useNagle;
#endif

		// Fields
		
		public const int DefaultNonPersistentConnectionLimit = 4;
		public const int DefaultPersistentConnectionLimit = 2;

		const string configKey = "system.net/connectionManagement";
		static ConnectionManagementData manager;
		
		static ServicePointManager ()
		{
#if NET_2_0 && CONFIGURATION_DEP
			object cfg = ConfigurationManager.GetSection (configKey);
			ConnectionManagementSection s = cfg as ConnectionManagementSection;
			if (s != null) {
				manager = new ConnectionManagementData (null);
				foreach (ConnectionManagementElement e in s.ConnectionManagement)
					manager.Add (e.Address, e.MaxConnection);

				return;
			}
#endif
			manager = (ConnectionManagementData) ConfigurationSettings.GetConfig (configKey);
		}

		// Constructors
		private ServicePointManager ()
		{
		}		
		
		// Properties
		
#if NET_2_0
		[Obsolete ("Use ServerCertificateValidationCallback instead",
			   false)]
#endif
		public static ICertificatePolicy CertificatePolicy {
			get { return policy; }
			set { policy = value; }
		}

#if NET_1_0
		// we need it for SslClientStream
		internal
#else
		[MonoTODO("CRL checks not implemented")]
		public
#endif
		static bool CheckCertificateRevocationList {
			get { return _checkCRL; }
			set { _checkCRL = false; }	// TODO - don't yet accept true
		}
		
		public static int DefaultConnectionLimit {
			get { return defaultConnectionLimit; }
			set { 
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				defaultConnectionLimit = value; 
			}
		}

#if NET_2_0
		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		[MonoTODO]
		public static int DnsRefreshTimeout
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
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
#endif
		
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
				RecycleServicePoints ();
			}
		}

#if NET_1_0
		// we need it for SslClientStream
		internal
#else
		public
#endif
		static SecurityProtocolType SecurityProtocol {
			get { return _securityProtocol; }
			set { _securityProtocol = value; }
		}

#if NET_2_0 && SECURITY_DEP
		[MonoTODO]
		public static RemoteCertificateValidationCallback ServerCertificateValidationCallback
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
#endif

#if NET_1_1
		public static bool Expect100Continue {
			get { return expectContinue; }
			set { expectContinue = value; }
		}

		public static bool UseNagleAlgorithm {
			get { return useNagle; }
			set { useNagle = value; }
		}
#endif
		// Methods
		
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

			RecycleServicePoints ();
			
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
			lock (servicePoints) {
				int key = address.GetHashCode () + (int) ((useConnect) ? 1 : 0);
				sp = servicePoints [key] as ServicePoint;
				if (sp != null)
					return sp;

				if (maxServicePoints > 0 && servicePoints.Count >= maxServicePoints)
					throw new InvalidOperationException ("maximum number of service points reached");

				string addr = address.ToString ();
				int limit = (int) manager.GetMaxConnections (addr);
				sp = new ServicePoint (address, limit, maxServicePointIdleTime);
#if NET_1_1
				sp.Expect100Continue = expectContinue;
				sp.UseNagleAlgorithm = useNagle;
#endif
				sp.UsesProxy = usesProxy;
				sp.UseConnect = useConnect;
				servicePoints.Add (key, sp);
			}
			
			return sp;
		}
		
		// Internal Methods

		internal static void RecycleServicePoints ()
		{
			ArrayList toRemove = new ArrayList ();
			lock (servicePoints) {
				IDictionaryEnumerator e = servicePoints.GetEnumerator ();
				while (e.MoveNext ()) {
					ServicePoint sp = (ServicePoint) e.Value;
					if (sp.AvailableForRecycling) {
						toRemove.Add (e.Key);
					}
				}
				
				for (int i = 0; i < toRemove.Count; i++) 
					servicePoints.Remove (toRemove [i]);

				if (maxServicePoints == 0 || servicePoints.Count <= maxServicePoints)
					return;

				// get rid of the ones with the longest idle time
				SortedList list = new SortedList (servicePoints.Count);
				e = servicePoints.GetEnumerator ();
				while (e.MoveNext ()) {
					ServicePoint sp = (ServicePoint) e.Value;
					if (sp.CurrentConnections == 0) {
						while (list.ContainsKey (sp.IdleSince))
							sp.IdleSince = sp.IdleSince.AddMilliseconds (1);
						list.Add (sp.IdleSince, sp.Address);
					}
				}
				
				for (int i = 0; i < list.Count && servicePoints.Count > maxServicePoints; i++)
					servicePoints.Remove (list.GetByIndex (i));
			}
		}
	}
}
