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
			ProcessRequestHandles = new List<ManualResetEvent> ();
		}

		public IChannelListener Listener { get; set; }
		public List<ManualResetEvent> ProcessRequestHandles { get; private set; }
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
		internal static SvcHttpHandler Current;

		static object type_lock = new object ();

		Type type;
		Type factory_type;
		string path;
		ServiceHostBase host;
		WcfListenerInfoCollection listeners = new WcfListenerInfoCollection ();
		Dictionary<HttpContext,ManualResetEvent> wcf_wait_handles = new Dictionary<HttpContext,ManualResetEvent> ();
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

		public HttpContext WaitForRequest (IChannelListener listener)
		{
			if (close_state > 0)
				return null;

			var info = listeners [listener];
			var ctx = info.Pending.Count == 0 ? null : info.Pending [0];
			if (ctx == null) {
				var wait = new ManualResetEvent (false);
				info.ProcessRequestHandles.Add (wait);
				wait.WaitOne ();
				ctx = info.Pending [0];
				info.ProcessRequestHandles.Remove (wait);
			}

			info.Pending.RemoveAt (0);
			return ctx;
		}

		IChannelListener FindBestMatchListener (HttpContext ctx)
		{
			var actx = new AspNetHttpContextInfo (ctx);

			// Select the best-match listener.
			IChannelListener best = null;
			string rel = null;
			foreach (var li in listeners) {
				var l = li.Listener;
				if (!l.GetProperty<HttpListenerManager> ().FilterHttpContext (actx))
					continue;
				if (l.Uri.Equals (ctx.Request.Url)) {
					best = l;
					break;
				}
			}
			// FIXME: the matching must be better-considered.
			foreach (var li in listeners) {
				var l = li.Listener;
				if (!l.GetProperty<HttpListenerManager> ().FilterHttpContext (actx))
					continue;
				if (!ctx.Request.Url.ToString ().StartsWith (l.Uri.ToString (), StringComparison.Ordinal))
					continue;
				if (best == null)
					best = l;
			}
			if (best != null)
				return best;
			throw new InvalidOperationException (String.Format ("The argument HTTP context did not match any of the registered listener manager (could be mismatch in URL, method etc.) {0}", ctx.Request.Url));
/*
			var actx = new AspNetHttpContextInfo (ctx);
			foreach (var i in listeners)
				if (i.Listener.GetProperty<HttpListenerManager> ().FilterHttpContext (actx))
					return i.Listener;
			throw new InvalidOperationException ();
*/
		}

		public void ProcessRequest (HttpContext context)
		{
			EnsureServiceHost ();

			var wait = new ManualResetEvent (false);
			var l = FindBestMatchListener (context);
			var i = listeners [l];
			lock (i) {
				i.Pending.Add (context);
				wcf_wait_handles [context] = wait;
				if (i.ProcessRequestHandles.Count > 0)
					i.ProcessRequestHandles [0].Set ();
			}

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
			lock (type_lock)
				listeners.Add (new WcfListenerInfo () {Listener = listener});
		}

		public void UnregisterListener (IChannelListener listener)
		{
			listeners.Remove (listener);
		}

		void EnsureServiceHost ()
		{
			lock (type_lock) {
				Current = this;
				try {
					EnsureServiceHostCore ();
				} finally {
					Current = null;
				}
			}
		}

		void EnsureServiceHostCore ()
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

			// Not precise, but it needs some wait time to have all channels start requesting. And it is somehow required.
			Thread.Sleep (500);
		}
	}
}
