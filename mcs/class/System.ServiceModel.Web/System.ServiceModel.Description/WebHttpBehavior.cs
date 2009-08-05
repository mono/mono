//
// WebHttpBehavior.cs
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
	public class WebHttpBehavior : IEndpointBehavior
	{
		public WebHttpBehavior ()
		{
			DefaultBodyStyle = WebMessageBodyStyle.Bare;
			DefaultOutgoingRequestFormat = WebMessageFormat.Xml;
			DefaultOutgoingResponseFormat = WebMessageFormat.Xml;
		}

		public virtual WebMessageBodyStyle DefaultBodyStyle { get; set; }

		public virtual WebMessageFormat DefaultOutgoingRequestFormat { get; set; }

		public virtual WebMessageFormat DefaultOutgoingResponseFormat { get; set; }

		public virtual void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			// nothing
		}

		[MonoTODO]
		protected virtual void AddClientErrorInspector (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			// clientRuntime.MessageInspectors.Add (something);
		}

		[MonoTODO]
		protected virtual void AddServerErrorHandlers (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			// endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add (something);
		}

		[MonoTODO ("where should I set reply client formatter?")]
		public virtual void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			AddClientErrorInspector (endpoint, clientRuntime);

			foreach (ClientOperation oper in clientRuntime.Operations) {
				oper.Formatter = GetRequestClientFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint);
				//oper.Formatter = GetReplyClientFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint); // FIXME: see MonoTODO.
			}
		}

		[MonoTODO ("where should I set reply dispatch formatter?")]
		public virtual void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.DispatchRuntime.OperationSelector = GetOperationSelector (endpoint);
			// FIXME: get HostNameComparisonMode from WebHttpBinding by some means.
			endpointDispatcher.AddressFilter = new PrefixEndpointAddressMessageFilter (endpoint.Address);
			endpointDispatcher.ContractFilter = new MatchAllMessageFilter ();
			AddServerErrorHandlers (endpoint, endpointDispatcher);

			foreach (DispatchOperation oper in endpointDispatcher.DispatchRuntime.Operations) {
				oper.Formatter = GetRequestDispatchFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint);
				//oper.Formatter = GetReplyDispatchFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint); // FIXME: see MonoTODO.
			}
		}

		protected virtual WebHttpDispatchOperationSelector GetOperationSelector (ServiceEndpoint endpoint)
		{
			return new WebHttpDispatchOperationSelector (endpoint);
		}

		protected virtual QueryStringConverter GetQueryStringConverter (OperationDescription operationDescription)
		{
			return new QueryStringConverter ();
		}

		protected virtual IClientMessageFormatter GetReplyClientFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.ReplyClientFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}

		protected virtual IDispatchMessageFormatter GetReplyDispatchFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.ReplyDispatchFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}

		protected virtual IClientMessageFormatter GetRequestClientFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.RequestClientFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}

		protected virtual IDispatchMessageFormatter GetRequestDispatchFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.RequestDispatchFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}

		[MonoTODO ("check UriTemplate validity")]
		public virtual void Validate (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			ValidateBinding (endpoint);
		}

		protected virtual void ValidateBinding (ServiceEndpoint endpoint)
		{
			switch (endpoint.Binding.Scheme) {
			case "http":
			case "https":
				break;
			default:
				throw new InvalidOperationException ("Only http and https are allowed for WebHttpBehavior");
			}
			if (!endpoint.Binding.MessageVersion.Equals (MessageVersion.None))
				throw new InvalidOperationException ("Only MessageVersion.None is allowed for WebHttpBehavior");
			if (!endpoint.Binding.CreateBindingElements ().Find<TransportBindingElement> ().ManualAddressing)
				throw new InvalidOperationException ("ManualAddressing in the transport binding element in the binding must be true for WebHttpBehavior");
		}
	}
}
