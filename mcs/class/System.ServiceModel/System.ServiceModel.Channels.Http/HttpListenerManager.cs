//
// HttpListenerManager.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;

namespace System.ServiceModel.Channels.Http
{
	internal class HttpListenerManager
	{
		public HttpListenerManager (Uri uri)
		{
			var l = new HttpListener ();

			string uriString = uri.ToString ();
			if (!uriString.EndsWith ("/", StringComparison.Ordinal))
				uriString += "/"; // HttpListener requires this mess.

			l.Prefixes.Add (uriString);

			this.listener = l;
		}
		
		HttpListener listener;
		List<HttpChannelListenerEntry> entries = new List<HttpChannelListenerEntry> ();

		Thread loop;

		// FIXME: use timeout
		public void RegisterListener (ChannelDispatcher channel, TimeSpan timeout)
		{
			entries.Add (new HttpChannelListenerEntry (channel, new AutoResetEvent (false)));

			entries.Sort (HttpChannelListenerEntry.CompareEntries);

			if (entries.Count != 1)
				return;

			// Start here. It is shared between channel listeners
			// that share the same listen Uri. So there is no other appropriate place.
#if true
			loop = new Thread (new ThreadStart (delegate {
				listener.Start ();
				try {
					while (true)
						ProcessNewContext (listener.GetContext ());
				} catch (ThreadAbortException) {
					Thread.ResetAbort ();
				}
				listener.Stop ();
			}));
			loop.Start ();
#else
			listener.BeginGetContext (GetContextCompleted, null);
#endif
		}

		// FIXME: use timeout
		public void UnregisterListener (ChannelDispatcher channel, TimeSpan timeout)
		{
			var entry = entries.First (e => e.ChannelDispatcher == channel);
			entries.Remove (entry);

			// stop the server if there is no more registered listener.
			if (entries.Count > 0)
				return;

#if true
			loop.Abort ();
#else
			this.listener.Stop ();
#endif
		}
		
		void GetContextCompleted (IAsyncResult result)
		{
			var ctx = listener.EndGetContext (result);
			ProcessNewContext (ctx);
			// start another listening
			listener.BeginGetContext (GetContextCompleted, null);
		}

		void ProcessNewContext (HttpListenerContext ctx)
		{
			if (ctx == null)
				return;

			var ctxi = new HttpStandaloneContextInfo (ctx);
			var ce = SelectChannel (ctxi);
			if (ce == null)
				throw new InvalidOperationException ("HttpListenerContext does not match any of the registered channels");
			ce.ContextQueue.Enqueue (ctxi);
			ce.WaitHandle.Set ();
		}

		HttpChannelListenerEntry SelectChannel (HttpContextInfo ctx)
		{
			foreach (var e in entries)
				if (e.FilterHttpContext (ctx))
					return e;
			return null;
		}

		public bool TryDequeueRequest (ChannelDispatcher channel, TimeSpan timeout, out HttpContextInfo context)
		{
			DateTime start = DateTime.Now;

			context = null;
			var ce = entries.First (e => e.ChannelDispatcher == channel);
			lock (ce.RetrieverLock) {
				var q = ce.ContextQueue;
				if (q.Count == 0) {
					bool ret = ce.WaitHandle.WaitOne (timeout);
					return ret && TryDequeueRequest (channel, timeout - (DateTime.Now - start), out context); // recurse, am lazy :/
				}
				context = q.Dequeue ();
				return true;
			}
		}
	}
}

