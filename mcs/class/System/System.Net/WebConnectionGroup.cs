//
// System.Net.WebConnectionGroup
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
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
using System.Net.Configuration;
using System.Net.Sockets;

namespace System.Net
{
	class WebConnectionGroup
	{
		ServicePoint sPoint;
		string name;
		ArrayList connections;
		Random rnd;
		Queue queue;

		public WebConnectionGroup (ServicePoint sPoint, string name)
		{
			this.sPoint = sPoint;
			this.name = name;
			connections = new ArrayList (1);
			queue = new Queue ();
		}

		public void Close ()
		{
			//TODO: what do we do with the queue? Empty it out and abort the requests?
			//TODO: abort requests or wait for them to finish
			lock (connections) {
				WeakReference cncRef = null;

				int end = connections.Count;
				// ArrayList removed = null;
				for (int i = 0; i < end; i++) {
					cncRef = (WeakReference) connections [i];
					WebConnection cnc = cncRef.Target as WebConnection;
					if (cnc != null) {
						cnc.Close (false);
					}
				}
				connections.Clear ();
			}
		}

		public WebConnection GetConnection (HttpWebRequest request)
		{
			WebConnection cnc = null;
			lock (connections) {
				WeakReference cncRef = null;

				// Remove disposed connections
				int end = connections.Count;
				ArrayList removed = null;
				for (int i = 0; i < end; i++) {
					cncRef = (WeakReference) connections [i];
					cnc = cncRef.Target as WebConnection;
					if (cnc == null) {
						if (removed == null)
							removed = new ArrayList (1);

						removed.Add (i);
					}
				}

				if (removed != null) {
					for (int i = removed.Count - 1; i >= 0; i--)
						connections.RemoveAt ((int) removed [i]);
				}

				cnc = CreateOrReuseConnection (request);
			}

			return cnc;
		}

		static void PrepareSharingNtlm (WebConnection cnc, HttpWebRequest request)
		{
			if (!cnc.NtlmAuthenticated)
				return;

			bool needs_reset = false;
			NetworkCredential cnc_cred = cnc.NtlmCredential;
			NetworkCredential req_cred = request.Credentials.GetCredential (request.RequestUri, "NTLM");
			if (cnc_cred.Domain != req_cred.Domain || cnc_cred.UserName != req_cred.UserName ||
				cnc_cred.Password != req_cred.Password) {
				needs_reset = true;
			}
#if NET_1_1
			if (!needs_reset) {
				bool req_sharing = request.UnsafeAuthenticatedConnectionSharing;
				bool cnc_sharing = cnc.UnsafeAuthenticatedConnectionSharing;
				needs_reset = (req_sharing == false || req_sharing != cnc_sharing);
			}
#endif
			if (needs_reset) {
				cnc.Close (false); // closes the authenticated connection
				cnc.ResetNtlm ();
			}
		}

		WebConnection CreateOrReuseConnection (HttpWebRequest request)
		{
			// lock is up there.
			WebConnection cnc;
			WeakReference cncRef;

			int count = connections.Count;
			for (int i = 0; i < count; i++) {
				WeakReference wr = connections [i] as WeakReference;
				cnc = wr.Target as WebConnection;
				if (cnc == null) {
					connections.RemoveAt (i);
					count--;
					i--;
					continue;
				}

				if (cnc.Busy)
					continue;

				PrepareSharingNtlm (cnc, request);
				return cnc;
			}

			if (sPoint.ConnectionLimit > count) {
				cnc = new WebConnection (this, sPoint);
				connections.Add (new WeakReference (cnc));
				return cnc;
			}

			if (rnd == null)
				rnd = new Random ();

			int idx = (count > 1) ? rnd.Next (0, count - 1) : 0;
			cncRef = (WeakReference) connections [idx];
			cnc = cncRef.Target as WebConnection;
			if (cnc == null) {
				cnc = new WebConnection (this, sPoint);
				connections.RemoveAt (idx);
				connections.Add (new WeakReference (cnc));
			}
			return cnc;
		}

		public string Name {
			get { return name; }
		}

		internal Queue Queue {
			get { return queue; }
		}
		
	}
}

