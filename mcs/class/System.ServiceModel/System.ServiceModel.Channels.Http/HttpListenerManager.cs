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
	internal abstract class HttpListenerManager
	{
		protected HttpListenerManager ()
		{
			Entries = new List<HttpChannelListenerEntry> ();
		}

		public List<HttpChannelListenerEntry> Entries { get; private set; }

		public abstract void RegisterListener (ChannelDispatcher channel, HttpTransportBindingElement element, TimeSpan timeout);
		public abstract void UnregisterListener (ChannelDispatcher channel, TimeSpan timeout);

		protected void RegisterListenerCommon (ChannelDispatcher channel, TimeSpan timeout)
		{
			Entries.Add (new HttpChannelListenerEntry (channel, new AutoResetEvent (false)));

			Entries.Sort (HttpChannelListenerEntry.CompareEntries);
		}

		protected void UnregisterListenerCommon (ChannelDispatcher channel, TimeSpan timeout)
		{
			var entry = Entries.First (e => e.ChannelDispatcher == channel);
			Entries.Remove (entry);

			entry.WaitHandle.Set (); // make sure to finish pending requests.
		}

		public void ProcessNewContext (HttpContextInfo ctxi)
		{
			var ce = SelectChannel (ctxi);
			if (ce == null)
				throw new InvalidOperationException ("HttpListenerContext does not match any of the registered channels");
			ce.ContextQueue.Enqueue (ctxi);
			ce.WaitHandle.Set ();
		}

		HttpChannelListenerEntry SelectChannel (HttpContextInfo ctx)
		{
			foreach (var e in Entries)
				if (e.FilterHttpContext (ctx))
					return e;
			return null;
		}

		public bool TryDequeueRequest (ChannelDispatcher channel, TimeSpan timeout, out HttpContextInfo context)
		{
			DateTime start = DateTime.Now;

			context = null;
			var ce = Entries.FirstOrDefault (e => e.ChannelDispatcher == channel);
			if (ce == null)
				return false;
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

	internal class HttpStandaloneListenerManager : HttpListenerManager
	{
		public HttpStandaloneListenerManager (Uri uri, HttpTransportBindingElement element)
		{
			var l = new HttpListener ();

#if false // FIXME: enable this once we found out why this causes problem
			string uriString = element.HostNameComparisonMode == HostNameComparisonMode.Exact ? uri.ToString () : uri.Scheme + "://*" + uri.GetComponents (UriComponents.Port | UriComponents.Path, UriFormat.SafeUnescaped);
#else
			string uriString = uri.ToString ();
#endif
			if (!uriString.EndsWith ("/", StringComparison.Ordinal))
				uriString += "/"; // HttpListener requires this mess.

			l.Prefixes.Add (uriString);

			this.listener = l;
		}
		
		HttpListener listener;

		Thread loop;

		// FIXME: use timeout
		public override void RegisterListener (ChannelDispatcher channel, HttpTransportBindingElement element, TimeSpan timeout)
		{
			RegisterListenerCommon (channel, timeout);

			if (Entries.Count != 1)
				return;

			if (element != null) {
				var l = listener;
				l.AuthenticationSchemeSelectorDelegate = delegate (HttpListenerRequest req) {
					return element.AuthenticationScheme;
				};
				l.Realm = element.Realm;
				l.UnsafeConnectionNtlmAuthentication = element.UnsafeConnectionNtlmAuthentication;
			}

			// Start here. It is shared between channel listeners
			// that share the same listen Uri. So there is no other appropriate place.
#if USE_SEPARATE_LOOP // this cannot be enabled because it causes infinite loop when ChannelDispatcher is not involved.
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
			listener.Start ();
			listener.BeginGetContext (GetContextCompleted, null);
#endif
		}

		// FIXME: use timeout
		public override void UnregisterListener (ChannelDispatcher channel, TimeSpan timeout)
		{
			UnregisterListenerCommon (channel, timeout);

			// stop the server if there is no more registered listener.
			if (Entries.Count > 0)
				return;

#if USE_SEPARATE_LOOP
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
			ProcessNewContext (new HttpStandaloneContextInfo (ctx));
		}
	}

	internal class AspNetHttpListenerManager : HttpListenerManager
	{
		public AspNetHttpListenerManager (Uri uri)
		{
		}

		public override void RegisterListener (ChannelDispatcher channel, HttpTransportBindingElement element, TimeSpan timeout)
		{
			RegisterListenerCommon (channel, timeout);
		}

		public override void UnregisterListener (ChannelDispatcher channel, TimeSpan timeout)
		{
			UnregisterListenerCommon (channel, timeout);
		}
	}
}

