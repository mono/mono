//
// WebScriptEnablingBehavior.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace System.ServiceModel.Description
{
	public sealed class WebScriptEnablingBehavior : WebHttpBehavior
	{
		public WebScriptEnablingBehavior ()
		{
			DefaultBodyStyle = WebMessageBodyStyle.WrappedRequest;
			DefaultOutgoingRequestFormat = WebMessageFormat.Json;
			DefaultOutgoingResponseFormat = WebMessageFormat.Json;
		}

		public override WebMessageBodyStyle DefaultBodyStyle { get; set; }

		public override WebMessageFormat DefaultOutgoingRequestFormat { get; set; }

		public override WebMessageFormat DefaultOutgoingResponseFormat { get; set; }

		[MonoTODO]
		protected override void AddClientErrorInspector (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		[MonoTODO]
		protected override void AddServerErrorHandlers (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}

		[MonoTODO]
		public override void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		public override void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
		}

		[MonoTODO]
		protected override QueryStringConverter GetQueryStringConverter (OperationDescription operationDescription)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("add non-XmlSerializer-ness check (but where?)")]
		public override void Validate (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			ValidateBinding (endpoint);

			foreach (var od in endpoint.Contract.Operations) {
				var wga = od.Behaviors.Find<WebGetAttribute> ();
				if (wga != null && wga.UriTemplate != null)
					throw new InvalidOperationException ("UriTemplate must not be used with WebScriptEnablingBehavior");
				var wia = od.Behaviors.Find<WebInvokeAttribute> ();
				if (wia != null) {
					if (wia.UriTemplate != null)
						throw new InvalidOperationException ("UriTemplate must not be used with WebScriptEnablingBehavior");
					switch (wia.Method.ToUpper ()) {
					case "GET":
					case "POST":
						break;
					default:
						throw new InvalidOperationException ("Only GET and POST HTTP methods are valid used for WebScriptEnablingBehavior");
					}
				}
			}
		}
	}
}
