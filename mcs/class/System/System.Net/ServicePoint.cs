//
// System.Net.ServicePoint
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net 
{
	public class ServicePoint
	{
		Uri uri;
		int connectionLimit;
		int maxIdleTime;
		int currentConnections;
		DateTime idleSince;
		Version protocolVersion;
		X509Certificate certificate;
		X509Certificate clientCertificate;
		IPHostEntry host;
		bool usesProxy;
		Hashtable groups;
		bool sendContinue = true;
		bool useConnect;
		object locker = new object ();
		object hostE = new object ();
#if NET_1_1
		bool useNagle;
#endif
#if NET_2_0
		BindIPEndPoint endPointCallback = null;
#endif
		
		// Constructors

		internal ServicePoint (Uri uri, int connectionLimit, int maxIdleTime)
		{
			this.uri = uri;  
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;			
			this.currentConnections = 0;
			this.idleSince = DateTime.Now;
		}
		
		// Properties
		
		public Uri Address {
			get { return uri; }
		}

#if NET_2_0
		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}

		public BindIPEndPoint BindIPEndPointDelegate
		{
			get { return endPointCallback; }
			set { endPointCallback = value; }
		}
#endif
		
		public X509Certificate Certificate {
			get { return certificate; }
		}
		
		public X509Certificate ClientCertificate {
			get { return clientCertificate; }
		}

#if NET_2_0
		[MonoTODO]
		public int ConnectionLeaseTimeout
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
#endif
		
		public int ConnectionLimit {
			get { return connectionLimit; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

				connectionLimit = value;
			}
		}
		
		public string ConnectionName {
			get { return uri.Scheme; }
		}

		public int CurrentConnections {
			get {
				return currentConnections;
			}
		}

		public DateTime IdleSince {
			get {
				return idleSince;
			}
			internal set {
				lock (locker)
					idleSince = value;
			}
		}
		
		public int MaxIdleTime {
			get { return maxIdleTime; }
			set { 
				if (value < Timeout.Infinite || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ();
				this.maxIdleTime = value; 
			}
		}
		
		public virtual Version ProtocolVersion {
			get { return protocolVersion; }
		}

#if NET_2_0
		[MonoTODO]
		public int ReceiveBufferSize
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
#endif
		
		public bool SupportsPipelining {
			get { return HttpVersion.Version11.Equals (protocolVersion); }
		}

#if NET_1_1
		public bool Expect100Continue {
			get { return SendContinue; }
			set { SendContinue = value; }
		}

		public bool UseNagleAlgorithm {
			get { return useNagle; }
			set { useNagle = value; }
		}
#endif

		internal bool SendContinue {
			get { return sendContinue &&
				     (protocolVersion == null || protocolVersion == HttpVersion.Version11); }
			set { sendContinue = value; }
		}
		// Methods
		
#if !NET_2_0
		public override int GetHashCode() 
		{
			return base.GetHashCode ();
		}
#endif
		
		// Internal Methods

		internal bool UsesProxy {
			get { return usesProxy; }
			set { usesProxy = value; }
		}

		internal bool UseConnect {
			get { return useConnect; }
			set { useConnect = value; }
		}

		internal bool AvailableForRecycling {
			get { 
				return CurrentConnections == 0
				    && maxIdleTime != Timeout.Infinite
			            && DateTime.Now >= IdleSince.AddMilliseconds (maxIdleTime);
			}
		}

		internal Hashtable Groups {
			get {
				if (groups == null)
					groups = new Hashtable ();

				return groups;
			}
		}

		internal IPHostEntry HostEntry
		{
			get {
				lock (hostE) {
					if (host != null)
						return host;

					string uriHost = uri.Host;

					// There is no need to do DNS resolution on literal IP addresses
					if (uri.HostNameType == UriHostNameType.IPv6 ||
						uri.HostNameType == UriHostNameType.IPv4) {

						if (uri.HostNameType == UriHostNameType.IPv6) {
							// Remove square brackets
							uriHost = uriHost.Substring(1,uriHost.Length-2);
						}

						// Creates IPHostEntry
						host = new IPHostEntry();
						host.AddressList = new IPAddress[] { IPAddress.Parse(uriHost) };

						return host;
					}

					// Try DNS resolution on host names
					try  {
						host = Dns.GetHostByName (uriHost);
					} 
					catch {
						return null;
					}
				}

				return host;
			}
		}

		internal void SetVersion (Version version)
		{
			protocolVersion = version;
		}

#if !TARGET_JVM
		WebConnectionGroup GetConnectionGroup (string name)
		{
			if (name == null)
				name = "";

			WebConnectionGroup group = Groups [name] as WebConnectionGroup;
			if (group != null)
				return group;

			group = new WebConnectionGroup (this, name);
			Groups [name] = group;
			return group;
		}

		internal EventHandler SendRequest (HttpWebRequest request, string groupName)
		{
			WebConnection cnc;
			
			lock (locker) {
				WebConnectionGroup cncGroup = GetConnectionGroup (groupName);
				cnc = cncGroup.GetConnection (request);
			}
			
			return cnc.SendRequest (request);
		}
#endif
#if NET_2_0
		public bool CloseConnectionGroup (string connectionGroupName)
		{
			lock (locker) {
				WebConnectionGroup cncGroup = GetConnectionGroup (connectionGroupName);
				if (cncGroup != null) {
					cncGroup.Close ();
					return true;
				}
			}

			return false;
		}
#endif

		internal void IncrementConnection ()
		{
			lock (locker) {
				currentConnections++;
				idleSince = DateTime.Now.AddMilliseconds (1000000);
			}
		}

		internal void DecrementConnection ()
		{
			lock (locker) {
				currentConnections--;
				if (currentConnections == 0)
					idleSince = DateTime.Now;
			}
		}

		internal void SetCertificates (X509Certificate client, X509Certificate server) 
		{
			certificate = server;
			clientCertificate = client;
		}

#if NET_2_0
		internal bool CallEndPointDelegate (Socket sock, IPEndPoint remote)
		{
			if (endPointCallback == null)
				return true;

			int count = 0;
			for (;;) {
				IPEndPoint local = null;
				try {
					local = endPointCallback (this,
						remote, count);
				} catch {
					// This is to differentiate from an
					// OverflowException, which should propagate.
					return false;
				}

				if (local == null)
					return true;

				try {
					sock.Bind (local);
				} catch (SocketException) {
					// This is intentional; the docs say
					// that if the Bind fails, we keep
					// going until there is an
					// OverflowException on the retry
					// count.
					checked { ++count; }
					continue;
				}

				return true;
			}
		}
#endif
	}
}


