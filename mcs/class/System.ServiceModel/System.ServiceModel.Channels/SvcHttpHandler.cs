//
// SvcHttpHandler.cs
//
// Author:
//	Ankit Jain  <jankit@novell.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2009 Novell, Inc.  http://www.novell.com
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
using System.Web;
using System.Threading;

using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels {

	internal class WcfListenerInfo
	{
		public WcfListenerInfo ()
		{
			Pending = new List<HttpContext> ();
		}

		public IChannelListener Listener { get; set; }
		public AutoResetEvent ProcessRequestHandle { get; set; }
		public List<HttpContext> Pending { get; private set; }
	}

	internal class WcfListenerInfoCollection : KeyedCollection<IChannelListener,WcfListenerInfo>
	{
		protected override IChannelListener GetKeyForItem (WcfListenerInfo info)
		{
			return info.Listener;
		}
	}

	internal class SvcHttpHandler : IHttpHandler
	{
		Type type;
		Type factory_type;
		string path;
		ServiceHostBase host;
		WcfListenerInfoCollection listeners = new WcfListenerInfoCollection ();
		Dictionary<HttpContext,AutoResetEvent> wcf_wait_handles = new Dictionary<HttpContext,AutoResetEvent> ();
		int close_state;

		public SvcHttpHandler (Type type, Type factoryType, string path)
		{
			this.type = type;
			this.factory_type = factoryType;
			this.path = path;
		}

		public bool IsReusable 
		{
			get { return true; }
		}

		public ServiceHostBase Host {
			get { return host; }
		}

		public HttpContext WaitForRequest (IChannelListener listener, TimeSpan timeout)
		{
			if (close_state > 0)
				return null;
			DateTime start = DateTime.Now;

			if (listeners [listener].Pending.Count == 0)
				listeners [listener].ProcessRequestHandle.WaitOne (timeout, false);

			if (listeners [listener].Pending.Count == 0)
				return null;

			var ctx = listeners [listener].Pending [0];
			listeners [listener].Pending.RemoveAt (0);
			return ctx;
		}

		IChannelListener FindBestMatchListener (HttpContext ctx)
		{
			// Select the best-match listener.
			IChannelListener best = null;
			string rel = null;
			foreach (var li in listeners) {
				var l = li.Listener;
				if (l.Uri.Equals (ctx.Request.Url)) {
					best = l;
					break;
				}
			}
			// FIXME: the matching must be better-considered.
			foreach (var li in listeners) {
				var l = li.Listener;
				if (!ctx.Request.Url.ToString ().StartsWith (l.Uri.ToString (), StringComparison.Ordinal))
					continue;
				if (best == null)
					best = l;
			}
			return best;
		}

		public void ProcessRequest (HttpContext context)
		{
			EnsureServiceHost ();

			var l = FindBestMatchListener (context);
			listeners [l].Pending.Add (context);
			listeners [l].ProcessRequestHandle.Set ();
			var wait = new AutoResetEvent (false);
			wcf_wait_handles [context] = wait;
			wait.WaitOne ();
		}

		public void EndRequest (IChannelListener listener, HttpContext context)
		{
			var wait = wcf_wait_handles [context];
			wcf_wait_handles.Remove (context);
			wait.Set ();
		}

		// called from SvcHttpHandlerFactory's remove callback (i.e.
		// unloading asp.net). It closes ServiceHost, then the host
		// in turn closes the listener and the channels it opened.
		// The channel listener calls CloseServiceChannel() to stop
		// accepting further requests on its shutdown.
		public void Close ()
		{
			host.Close ();
			host = null;
		}

		public void RegisterListener (IChannelListener listener)
		{
			listeners.Add (new WcfListenerInfo () {
				Listener = listener,
				ProcessRequestHandle = new AutoResetEvent (false) });
		}

		public void UnregisterListener (IChannelListener listener)
		{
			listeners.Remove (listener);
		}

		void EnsureServiceHost ()
		{
			if (host != null)
				return;

			//ServiceHost for this not created yet
			var baseUri = new Uri (new Uri (HttpContext.Current.Request.Url.GetLeftPart (UriPartial.Authority)), path);
			if (factory_type != null) {
				host = ((ServiceHostFactory) Activator.CreateInstance (factory_type)).CreateServiceHost (type, new Uri [] {baseUri});
			}
			else
				host = new ServiceHost (type, baseUri);
			host.Extensions.Add (new VirtualPathExtension (baseUri.AbsolutePath));

			host.Open ();
		}
	}
}
