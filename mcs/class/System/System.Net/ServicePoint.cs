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
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace System.Net
{
	public class ServicePoint
	{
		readonly Uri uri;
		DateTime lastDnsResolve;
		Version protocolVersion;
		IPHostEntry host;
		bool usesProxy;
		bool sendContinue = true;
		bool useConnect;
		object hostE = new object ();
		bool useNagle;
		BindIPEndPoint endPointCallback = null;
		bool tcp_keepalive;
		int tcp_keepalive_time;
		int tcp_keepalive_interval;
		bool disposed;
		int connectionLeaseTimeout = -1;
		int receiveBufferSize = -1;

		// Constructors

		internal ServicePoint (ServicePointManager.SPKey key, Uri uri, int connectionLimit, int maxIdleTime)
		{
			Key = key;
			this.uri = uri;
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;

			Scheduler = new ServicePointScheduler (this, connectionLimit, maxIdleTime);
		}

		internal ServicePointManager.SPKey Key {
			get;
		}

		ServicePointScheduler Scheduler {
			get; set;
		}

		// Properties

		public Uri Address {
			get { return uri; }
		}

		public BindIPEndPoint BindIPEndPointDelegate {
			get { return endPointCallback; }
			set { endPointCallback = value; }
		}

		public int ConnectionLeaseTimeout {
			get { return connectionLeaseTimeout; }
			set
			{
				if (value < Timeout.Infinite)
					throw new ArgumentOutOfRangeException (nameof (value));

				connectionLeaseTimeout = value;
			}
		}

		int connectionLimit;
		int maxIdleTime;

		public int ConnectionLimit {
			get { return connectionLimit; }
			set {
				connectionLimit = value;
				if (!disposed)
					Scheduler.ConnectionLimit = value;
			}
		}

		public string ConnectionName {
			get { return uri.Scheme; }
		}

		public int CurrentConnections {
			get {
				return disposed ? 0 : Scheduler.CurrentConnections;
			}
		}

		public DateTime IdleSince {
			get {
				if (disposed)
					return DateTime.MinValue;
				return Scheduler.IdleSince.ToLocalTime ();
			}
		}

		public int MaxIdleTime {
			get { return maxIdleTime; }
			set {
				maxIdleTime = value;
				if (!disposed)
					Scheduler.MaxIdleTime = value;
			}
		}

		public virtual Version ProtocolVersion {
			get { return protocolVersion; }
		}

		public int ReceiveBufferSize {
			get { return receiveBufferSize; }
			set
			{
				if (value < -1)
					throw new ArgumentOutOfRangeException (nameof (value));

				receiveBufferSize = value;
			}
		}

		public bool SupportsPipelining {
			get { return HttpVersion.Version11.Equals (protocolVersion); }
		}


		public bool Expect100Continue {
			get { return SendContinue; }
			set { SendContinue = value; }
		}

		public bool UseNagleAlgorithm {
			get { return useNagle; }
			set { useNagle = value; }
		}

		internal bool SendContinue {
			get {
				return sendContinue &&
				       (protocolVersion == null || protocolVersion == HttpVersion.Version11);
			}
			set { sendContinue = value; }
		}
		// Methods

		public void SetTcpKeepAlive (bool enabled, int keepAliveTime, int keepAliveInterval)
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

		internal void KeepAliveSetup (Socket socket)
		{
			if (!tcp_keepalive)
				return;

			byte[] bytes = new byte[12];
			PutBytes (bytes, (uint)(tcp_keepalive ? 1 : 0), 0);
			PutBytes (bytes, (uint)tcp_keepalive_time, 4);
			PutBytes (bytes, (uint)tcp_keepalive_interval, 8);
			socket.IOControl (IOControlCode.KeepAliveValues, bytes, null);
		}

		static void PutBytes (byte[] bytes, uint v, int offset)
		{
			if (BitConverter.IsLittleEndian) {
				bytes[offset] = (byte)(v & 0x000000ff);
				bytes[offset + 1] = (byte)((v & 0x0000ff00) >> 8);
				bytes[offset + 2] = (byte)((v & 0x00ff0000) >> 16);
				bytes[offset + 3] = (byte)((v & 0xff000000) >> 24);
			} else {
				bytes[offset + 3] = (byte)(v & 0x000000ff);
				bytes[offset + 2] = (byte)((v & 0x0000ff00) >> 8);
				bytes[offset + 1] = (byte)((v & 0x00ff0000) >> 16);
				bytes[offset] = (byte)((v & 0xff000000) >> 24);
			}
		}

		// Internal Methods

		internal bool UsesProxy {
			get { return usesProxy; }
			set { usesProxy = value; }
		}

		internal bool UseConnect {
			get { return useConnect; }
			set { useConnect = value; }
		}

		private bool HasTimedOut {
			get {
				int timeout = ServicePointManager.DnsRefreshTimeout;
				return timeout != Timeout.Infinite &&
					(lastDnsResolve + TimeSpan.FromMilliseconds (timeout)) < DateTime.UtcNow;
			}
		}

		internal IPHostEntry HostEntry {
			get {
				lock (hostE) {
					string uriHost = uri.Host;

					// Cannot do DNS resolution on literal IP addresses
					if (uri.HostNameType == UriHostNameType.IPv6 || uri.HostNameType == UriHostNameType.IPv4) {
						if (host != null)
							return host;

						if (uri.HostNameType == UriHostNameType.IPv6) {
							// Remove square brackets
							uriHost = uriHost.Substring (1, uriHost.Length - 2);
						}

						// Creates IPHostEntry
						host = new IPHostEntry ();
						host.AddressList = new IPAddress[] { IPAddress.Parse (uriHost) };
						return host;
					}

					if (!HasTimedOut && host != null)
						return host;

					lastDnsResolve = DateTime.UtcNow;

					try {
						host = Dns.GetHostEntry (uriHost);
					} catch {
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

		internal void SendRequest (WebOperation operation, string groupName)
		{
			lock (this) {
				if (disposed)
					throw new ObjectDisposedException (typeof (ServicePoint).FullName);
				Scheduler.SendRequest (operation, groupName);
			}
		}

		public bool CloseConnectionGroup (string connectionGroupName)
		{
			lock (this) {
				if (disposed)
					return true;
				return Scheduler.CloseConnectionGroup (connectionGroupName);
			}
		}

		internal void FreeServicePoint ()
		{
			disposed = true;
			Scheduler = null;
		}

		//
		// Copied from the referencesource
		//

		object m_ServerCertificateOrBytes;
		object m_ClientCertificateOrBytes;

		/// <devdoc>
		///    <para>
		///       Gets the certificate received for this <see cref='System.Net.ServicePoint'/>.
		///    </para>
		/// </devdoc>
		public  X509Certificate Certificate {
			get {
				object chkCert = m_ServerCertificateOrBytes;
				if (chkCert != null && chkCert.GetType() == typeof(byte[]))
					return (X509Certificate)(m_ServerCertificateOrBytes = new X509Certificate((byte[]) chkCert));
				else
					return chkCert as X509Certificate;
			}
		}
		internal void UpdateServerCertificate(X509Certificate certificate)
		{
			if (certificate != null)
				m_ServerCertificateOrBytes = certificate.GetRawCertData();
			else
				m_ServerCertificateOrBytes = null;
		}

		/// <devdoc>
		/// <para>
		/// Gets the Client Certificate sent by us to the Server.
		/// </para>
		/// </devdoc>
		public  X509Certificate ClientCertificate {
			get {
				object chkCert = m_ClientCertificateOrBytes;
				if (chkCert != null && chkCert.GetType() == typeof(byte[]))
					return (X509Certificate)(m_ClientCertificateOrBytes = new X509Certificate((byte[]) chkCert));
				else
					return chkCert as X509Certificate;
			}
		}
		internal void UpdateClientCertificate(X509Certificate certificate)
		{
			if (certificate != null)
				m_ClientCertificateOrBytes = certificate.GetRawCertData();
			else
				m_ClientCertificateOrBytes = null;
		}

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

		internal Socket GetConnection(PooledStream PooledStream, object owner, bool async, out IPAddress address, ref Socket abortSocket, ref Socket abortSocket6)
		{
			throw new NotImplementedException ();
		}
	}
}


