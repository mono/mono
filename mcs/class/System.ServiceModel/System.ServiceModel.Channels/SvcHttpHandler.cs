//
// SvcHttpHandler.cs
//
// Author:
//	Ankit Jain  <jankit@novell.com>
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2009-2010 Novell, Inc.  http://www.novell.com
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
using System.ServiceModel.Channels.Http;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	internal class SvcHttpHandler : IHttpHandler
	{
		internal static SvcHttpHandler Current;

		static object type_lock = new object ();

		Type type;
		Type factory_type;
		string path;
		ServiceHostBase host;
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

		public void ProcessRequest (HttpContext context)
		{
			EnsureServiceHost ();

			var table = HttpListenerManagerTable.GetOrCreate (host);
			var manager = table.GetOrCreateManager (context.Request.Url);
			if (manager == null)
				manager = table.GetOrCreateManager (host.BaseAddresses [0]);
			var wait = new ManualResetEvent (false);
			wcf_wait_handles [context] = wait;
			manager.ProcessNewContext (new System.ServiceModel.Channels.Http.AspNetHttpContextInfo (this, context));
			// This method must not return until the RequestContext
			// explicitly finishes replying. Otherwise xsp will
			// close the connection after this method call.
			wait.WaitOne ();
		}

		public void EndHttpRequest (HttpContext context)
		{
			ManualResetEvent wait;
			if (!wcf_wait_handles.TryGetValue (context, out wait))
				return;

			wcf_wait_handles.Remove (context);
			if (wait != null)
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
//			if (factory_type != null) {
//				host = ((ServiceHostFactory) Activator.CreateInstance (factory_type)).CreateServiceHost (type, new Uri [] {baseUri});
//			}
//			else
				host = new ServiceHost (type, baseUri);
			host.Extensions.Add (new VirtualPathExtension (baseUri.AbsolutePath));

			host.Open ();
		}
	}
}
