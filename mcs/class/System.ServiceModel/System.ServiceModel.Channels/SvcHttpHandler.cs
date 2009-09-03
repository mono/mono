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
using System.Web;
using System.Threading;

using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels {

	internal class SvcHttpHandler : IHttpHandler
	{
		Type type;
		Type factory_type;
		string path;
		Uri request_url;
		ServiceHostBase host;
		Queue<HttpContext> pending = new Queue<HttpContext> ();
		bool closing;

		AutoResetEvent wait = new AutoResetEvent (false);
		AutoResetEvent listening = new AutoResetEvent (false);

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

		public Uri Uri { get; private set; }

		public HttpContext WaitForRequest (TimeSpan timeout)
		{
			DateTime start = DateTime.Now;
			lock (pending) {
				if (pending.Count > 0) {
					var ctx = pending.Dequeue ();
					if (ctx.AllErrors != null && ctx.AllErrors.Length > 0)
						return WaitForRequest (timeout - (DateTime.Now - start));
					return ctx;
				}
			}

			return wait.WaitOne (timeout - (DateTime.Now - start), false) && !closing ?
				WaitForRequest (timeout - (DateTime.Now - start)) : null;
		}

		public void ProcessRequest (HttpContext context)
		{
			request_url = context.Request.Url;
			EnsureServiceHost ();
			pending.Enqueue (context);

			wait.Set ();

			listening.WaitOne ();
		}

		public void EndRequest (HttpContext context)
		{
			listening.Set ();
		}

		public void Close ()
		{
			closing = true;
			listening.Set ();
			wait.Set ();
			host.Close ();
			host = null;
			closing = false;
		}

		void ApplyConfiguration (ServiceHost host)
		{
			foreach (ServiceElement service in ConfigUtil.ServicesSection.Services) {
				foreach (ServiceEndpointElement endpoint in service.Endpoints) {
					// FIXME: consider BindingName as well
					ServiceEndpoint se = host.AddServiceEndpoint (
						endpoint.Contract,
						ConfigUtil.CreateBinding (endpoint.Binding, endpoint.BindingConfiguration),
						new Uri (path));
					this.Uri = se.Address.Uri;
				}
				// behaviors
				ServiceBehaviorElement behavior = ConfigUtil.BehaviorsSection.ServiceBehaviors.Find (service.BehaviorConfiguration);
				if (behavior != null) {
					foreach (BehaviorExtensionElement bxel in behavior) {
						IServiceBehavior b = null;
						ServiceMetadataPublishingElement meta = bxel as ServiceMetadataPublishingElement;
						if (meta != null) {
							ServiceMetadataBehavior smb = meta.CreateBehavior () as ServiceMetadataBehavior;
							smb.HttpGetUrl = request_url;
							// FIXME: HTTPS as well
							b = smb;
						}
						if (b != null)
							host.Description.Behaviors.Add (b);
					}
				}
			}
		}

		void EnsureServiceHost ()
		{
			if (host != null)
				return;

			//ServiceHost for this not created yet
			var baseUri = new Uri (HttpContext.Current.Request.Url.GetLeftPart (UriPartial.Path));
			if (factory_type != null) {
				host = ((ServiceHostFactory) Activator.CreateInstance (factory_type)).CreateServiceHost (type, new Uri [] {baseUri});
			}
			else
				host = new ServiceHost (type, baseUri);

#if true
			//FIXME: Binding: Get from web.config.
			var se = host.AddServiceEndpoint (ContractDescription.GetContract (type).Name,
				new BasicHttpBinding (), new Uri (path, UriKind.Relative));
			this.Uri = se.Address.Uri;
#else
			ApplyConfiguration (host);
#endif

			host.Open ();

			//listening.WaitOne ();
		}
	}
}
