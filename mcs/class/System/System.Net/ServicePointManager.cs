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

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
		internal class SPKey {
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

		static ConcurrentDictionary<SPKey, ServicePoint> servicePoints = new ConcurrentDictionary<SPKey, ServicePoint> ();

		// Static properties
		
		private static ICertificatePolicy policy;
		private static int defaultConnectionLimit = DefaultPersistentConnectionLimit;
		private static int maxServicePointIdleTime = 100000; // 100 seconds
		private static int maxServicePoints = 0;
		private static int dnsRefreshTimeout = 2 * 60 * 1000;
		private static bool _checkCRL = false;
		private static SecurityProtocolType _securityProtocol = SecurityProtocolType.SystemDefault;

		static bool expectContinue = true;
		static bool useNagle;
		static ServerCertValidationCallback server_cert_cb;
		static bool tcp_keepalive;
		static int tcp_keepalive_time;
		static int tcp_keepalive_interval;

		// Fields
		
		public const int DefaultNonPersistentConnectionLimit = 4;
#if MOBILE
		public const int DefaultPersistentConnectionLimit = 10;
#else
		public const int DefaultPersistentConnectionLimit = 2;
#endif

#if !MOBILE
		const string configKey = "system.net/connectionManagement";
		static ConnectionManagementData manager;
#endif
		
		static ServicePointManager ()
		{
#if !MOBILE
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

#pragma warning disable 618
			manager = (ConnectionManagementData) ConfigurationSettings.GetConfig (configKey);
#pragma warning restore 618
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
			get {
				if (policy == null)
					Interlocked.CompareExchange (ref policy, new DefaultCertificatePolicy (), null);
				return policy;
			}
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
#if !MOBILE
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

		[MonoTODO]
		public static bool ReusePort {
			get { return false; }
			set { throw new NotImplementedException (); }
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

		[MonoTODO ("Always returns EncryptionPolicy.RequireEncryption.")]
		public static EncryptionPolicy EncryptionPolicy {
			get {
				return EncryptionPolicy.RequireEncryption;
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

		internal static bool DisableSendAuxRecord {
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
			return FindServicePoint (address, null);
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
				if (address.Scheme != "http")
					throw new NotSupportedException ("Proxy scheme not supported.");

				if (isSecure && address.Scheme == "http")
					useConnect = true;
			} 

			address = new Uri (address.Scheme + "://" + address.Authority);
			
			var key = new SPKey (origAddress, usesProxy ? address : null, useConnect);
			lock (servicePoints) {
				if (servicePoints.TryGetValue (key, out var sp))
					return sp;

				if (maxServicePoints > 0 && servicePoints.Count >= maxServicePoints)
					throw new InvalidOperationException ("maximum number of service points reached");

				int limit;
#if MOBILE
				limit = defaultConnectionLimit;
#else
				string addr = address.ToString ();
				limit = (int) manager.GetMaxConnections (addr);
#endif
				sp = new ServicePoint (key, address, limit, maxServicePointIdleTime);
				sp.Expect100Continue = expectContinue;
				sp.UseNagleAlgorithm = useNagle;
				sp.UsesProxy = usesProxy;
				sp.UseConnect = useConnect;
				sp.SetTcpKeepAlive (tcp_keepalive, tcp_keepalive_time, tcp_keepalive_interval);

				return servicePoints.GetOrAdd (key, sp);
			}
		}

		internal static void CloseConnectionGroup (string connectionGroupName)
		{
			lock (servicePoints) {
				foreach (ServicePoint sp in servicePoints.Values) {
					sp.CloseConnectionGroup (connectionGroupName);
				}
			}
		}

		internal static void RemoveServicePoint (ServicePoint sp)
		{
			servicePoints.TryRemove (sp.Key, out var value);
		}
	}
}

