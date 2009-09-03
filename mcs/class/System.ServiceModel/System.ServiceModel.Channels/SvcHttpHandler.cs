//
// SvcHttpHandler.cs
//
// Author:
//	Ankit Jain  <jankit@novell.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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

		AspNetReplyChannel reply_channel;
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

		public bool WaitForRequest (AspNetReplyChannel reply_channel, TimeSpan timeout)
		{
			this.reply_channel = reply_channel;
			listening.Set ();

			return wait.WaitOne (timeout, false);
		}

		public void ProcessRequest (HttpContext context)
		{
			request_url = context.Request.Url;
			EnsureServiceHost ();

			reply_channel.Context = context;
			wait.Set ();

			listening.WaitOne ();
			reply_channel.Context = null;
		}

		public void Close ()
		{
			host.Close ();
			host = null;
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
			if (reply_channel != null)
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
			host.AddServiceEndpoint (ContractDescription.GetContract (type).Name,
				new BasicHttpBinding (), new Uri (path, UriKind.Relative));
#else
			ApplyConfiguration (host);
#endif

			host.Open ();

			listening.WaitOne ();
		}
	}
}
