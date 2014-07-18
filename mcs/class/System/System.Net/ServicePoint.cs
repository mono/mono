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
		Dictionary<string,WebConnectionGroup> groups;
		bool sendContinue = true;
		bool useConnect;
		object hostE = new object ();
		bool useNagle;
		BindIPEndPoint endPointCallback = null;
		bool tcp_keepalive;
		int tcp_keepalive_time;
		int tcp_keepalive_interval;
		Timer idleTimer;

		// Constructors

		internal ServicePoint (Uri uri, int connectionLimit, int maxIdleTime)
		{
			this.uri = uri;  
			this.connectionLimit = connectionLimit;
			this.maxIdleTime = maxIdleTime;	
			this.currentConnections = 0;
			this.idleSince = DateTime.UtcNow;
		}
		
		// Properties
		
		public Uri Address {
			get { return uri; }
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}

		public BindIPEndPoint BindIPEndPointDelegate
		{
			get { return endPointCallback; }
			set { endPointCallback = value; }
		}
		
		public X509Certificate Certificate {
			get { return certificate; }
		}
		
		public X509Certificate ClientCertificate {
			get { return clientCertificate; }
		}

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
				return idleSince.ToLocalTime ();
			}
		}

		public int MaxIdleTime {
			get { return maxIdleTime; }
			set { 
				if (value < Timeout.Infinite || value > Int32.MaxValue)
					throw new ArgumentOutOfRangeException ();

				lock (this) {
					maxIdleTime = value;
					if (idleTimer != null)
						idleTimer.Change (maxIdleTime, maxIdleTime);
				}
			}
		}
		
		public virtual Version ProtocolVersion {
			get { return protocolVersion; }
		}

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
			get { return sendContinue &&
				     (protocolVersion == null || protocolVersion == HttpVersion.Version11); }
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

			byte [] bytes = new byte [12];
			PutBytes (bytes, (uint) (tcp_keepalive ? 1 : 0), 0);
			PutBytes (bytes, (uint) tcp_keepalive_time, 4);
			PutBytes (bytes, (uint) tcp_keepalive_interval, 8);
			socket.IOControl (IOControlCode.KeepAliveValues, bytes, null);
		}

		static void PutBytes (byte [] bytes, uint v, int offset)
		{
			if (BitConverter.IsLittleEndian) {
				bytes [offset] = (byte) (v & 0x000000ff);
				bytes [offset + 1] = (byte) ((v & 0x0000ff00) >> 8);
				bytes [offset + 2] = (byte) ((v & 0x00ff0000) >> 16);
				bytes [offset + 3] = (byte) ((v & 0xff000000) >> 24);
			} else {
				bytes [offset + 3] = (byte) (v & 0x000000ff);
				bytes [offset + 2] = (byte) ((v & 0x0000ff00) >> 8);
				bytes [offset + 1] = (byte) ((v & 0x00ff0000) >> 16);
				bytes [offset] = (byte) ((v & 0xff000000) >> 24);
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

		WebConnectionGroup GetConnectionGroup (string name)
		{
			if (name == null)
				name = "";

			/*
			 * Optimization:
			 * 
			 * In the vast majority of cases, we only have one single WebConnectionGroup per ServicePoint, so we
			 * don't need to allocate a dictionary.
			 * 
			 */

			WebConnectionGroup group;
			if (groups != null && groups.TryGetValue (name, out group))
				return group;

			group = new WebConnectionGroup (this, name);
			group.ConnectionClosed += (s, e) => currentConnections--;

			if (groups == null)
				groups = new Dictionary<string, WebConnectionGroup> ();
			groups.Add (name, group);

			return group;
		}

		void RemoveConnectionGroup (WebConnectionGroup group)
		{
			if (groups == null || groups.Count == 0)
				throw new InvalidOperationException ();

			groups.Remove (group.Name);
		}

		internal bool CheckAvailableForRecycling (out DateTime outIdleSince)
		{
			outIdleSince = DateTime.MinValue;

			TimeSpan idleTimeSpan;
			List<WebConnectionGroup> groupList = null, removeList = null;
			lock (this) {
				if (groups == null || groups.Count == 0) {
					idleSince = DateTime.MinValue;
					return true;
				}

				idleTimeSpan = TimeSpan.FromMilliseconds (maxIdleTime);

				/*
				 * WebConnectionGroup.TryRecycle() must run outside the lock, so we need to
				 * copy the group dictionary if it exists.
				 * 
				 * In most cases, we only have a single connection group, so we can simply store
				 * that in a local variable instead of copying a collection.
				 * 
				 */

				groupList = new List<WebConnectionGroup> (groups.Values);
			}

			foreach (var group in groupList) {
				if (!group.TryRecycle (idleTimeSpan, ref outIdleSince))
					continue;
				if (removeList == null)
					removeList = new List<WebConnectionGroup> ();
				removeList.Add (group);
			}

			lock (this) {
				idleSince = outIdleSince;

				if (removeList != null) {
					foreach (var group in removeList)
						RemoveConnectionGroup (group);
				}

				if (groups != null && groups.Count == 0)
					groups = null;

				if (groups == null) {
					if (idleTimer != null) {
						idleTimer.Dispose ();
						idleTimer = null;
					}
					return true;
				}

				return false;
			}
		}

		void IdleTimerCallback (object obj)
		{
			DateTime dummy;
			CheckAvailableForRecycling (out dummy);
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
		internal EventHandler SendRequest (HttpWebRequest request, string groupName)
		{
			WebConnection cnc;
			
			lock (this) {
				bool created;
				WebConnectionGroup cncGroup = GetConnectionGroup (groupName);
				cnc = cncGroup.GetConnection (request, out created);
				if (created) {
					++currentConnections;
					if (idleTimer == null)
						idleTimer = new Timer (IdleTimerCallback, null, maxIdleTime, maxIdleTime);
				}
			}
			
			return cnc.SendRequest (request);
		}
#endif
		public bool CloseConnectionGroup (string connectionGroupName)
		{
			lock (this) {
				WebConnectionGroup cncGroup = GetConnectionGroup (connectionGroupName);
				if (cncGroup != null) {
					cncGroup.Close ();
					return true;
				}
			}

			return false;
		}

		internal void SetCertificates (X509Certificate client, X509Certificate server) 
		{
			certificate = server;
			clientCertificate = client;
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
	}
}


