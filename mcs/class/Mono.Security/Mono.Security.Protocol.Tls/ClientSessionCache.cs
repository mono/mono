//
// ClientSessionCache.cs: Client-side cache for re-using sessions
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell (http://www.novell.com)
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

namespace Mono.Security.Protocol.Tls {

	internal class ClientSessionInfo : IDisposable {

		// (by default) we keep this item valid for 3 minutes (if unused)
		private const int DefaultValidityInterval = 3 * 60;
		private static readonly int ValidityInterval;

		private bool disposed;
		private DateTime validuntil;
		private string host;

		// see RFC2246 - Section 7
		private byte[] sid;
		private byte[] masterSecret;

		static ClientSessionInfo ()
		{
#if MOONLIGHT
			ValidityInterval = DefaultValidityInterval;
#else
			string user_cache_timeout = Environment.GetEnvironmentVariable ("MONO_TLS_SESSION_CACHE_TIMEOUT");
			if (user_cache_timeout == null) {
				ValidityInterval = DefaultValidityInterval;
			} else {
				try {
					ValidityInterval = Int32.Parse (user_cache_timeout);
				}
				catch {
					ValidityInterval = DefaultValidityInterval;
				}
			}
#endif
		}

		public ClientSessionInfo (string hostname, byte[] id)
		{
			host = hostname;
			sid = id;
			KeepAlive ();
		}

		~ClientSessionInfo ()
		{
			Dispose (false);
		}


		public string HostName {
			get { return host; }
		}

		public byte[] Id {
			get { return sid; }
		}

		public bool Valid {
			get { return ((masterSecret != null) && (validuntil > DateTime.UtcNow)); }
		}


		public void GetContext (Context context)
		{
			CheckDisposed ();
			if (context.MasterSecret != null)
				masterSecret = (byte[]) context.MasterSecret.Clone ();
		}

		public void SetContext (Context context)
		{
			CheckDisposed ();
			if (masterSecret != null)
				context.MasterSecret = (byte[]) masterSecret.Clone ();
		}

		public void KeepAlive ()
		{
			CheckDisposed ();
			validuntil = DateTime.UtcNow.AddSeconds (ValidityInterval);
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		private void Dispose (bool disposing)
		{
			if (!disposed) {
				validuntil = DateTime.MinValue;
				host = null;
				sid = null;

				if (masterSecret != null) {
					Array.Clear (masterSecret, 0, masterSecret.Length);
					masterSecret = null;
				}
			}
			disposed = true;
		}

		private void CheckDisposed ()
		{
			if (disposed) {
				string msg = Locale.GetText ("Cache session information were disposed.");
				throw new ObjectDisposedException (msg);
			}
		}
	}

	// note: locking is aggressive but isn't used often (and we gain much more :)
	internal class ClientSessionCache {

		static Hashtable cache;
		static object locker;

		static ClientSessionCache ()
		{
			cache = new Hashtable ();
			locker = new object ();
		}

		// note: we may have multiple connections with a host, so 
		// possibly multiple entries per host (each with a different 
		// id), so we do not use the host as the hashtable key
		static public void Add (string host, byte[] id)
		{
			lock (locker) {
				string uid = BitConverter.ToString (id);
				ClientSessionInfo si = (ClientSessionInfo) cache[uid];
				if (si == null) {
					cache.Add (uid, new ClientSessionInfo (host, id));
				} else if (si.HostName == host) {
					// we already have this and it's still valid
					// on the server, so we'll keep it a little longer
					si.KeepAlive ();
				} else {
					// it's very unlikely but the same session id 
					// could be used by more than one host. In this
					// case we replace the older one with the new one
					si.Dispose ();
					cache.Remove (uid);
					cache.Add (uid, new ClientSessionInfo (host, id));
				}
			}
		}

		// return the first session us
		static public byte[] FromHost (string host)
		{
			lock (locker) {
				foreach (ClientSessionInfo si in cache.Values) {
					if (si.HostName == host) {
						if (si.Valid) {
							// ensure it's still valid when we really need it
							si.KeepAlive ();
							return si.Id;
						}
					}
				}
				return null;
			}
		}

		// only called inside the lock
		static private ClientSessionInfo FromContext (Context context, bool checkValidity)
		{
			if (context == null)
				return null;

			byte[] id = context.SessionId;
			if ((id == null) || (id.Length == 0))
				return null;

			// do we have a session cached for this host ?
			string uid = BitConverter.ToString (id);

			ClientSessionInfo si = (ClientSessionInfo) cache[uid];
			if (si == null)
				return null;

			// In the unlikely case of multiple hosts using the same 
			// session id, we just act like we do not know about it
			if (context.ClientSettings.TargetHost != si.HostName)
				return null;

			// yes, so what's its status ?
			if (checkValidity && !si.Valid) {
				si.Dispose ();
				cache.Remove (uid);
				return null;
			}

			// ok, it make sense
			return si;
		}

		static public bool SetContextInCache (Context context)
		{
			lock (locker) {
				// Don't check the validity because the masterKey of the ClientSessionInfo
				// can still be null when this is called the first time
				ClientSessionInfo csi = FromContext (context, false);
				if (csi == null)
					return false;

				csi.GetContext (context);
				csi.KeepAlive ();
				return true;
			}
		}

		static public bool SetContextFromCache (Context context)
		{
			lock (locker) {
				ClientSessionInfo csi = FromContext (context, true);
				if (csi == null)
					return false;

				csi.SetContext (context);
				csi.KeepAlive ();
				return true;
			}
		}
	}
}
