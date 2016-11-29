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
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;

namespace System.ServiceModel.Description
{

	internal static class WebHttpBehaviorExtensions
	{
		public static WebAttributeInfo GetWebAttributeInfo (this OperationDescription od)
		{
			foreach (IOperationBehavior ob in od.Behaviors) {
				var wg = ob as WebGetAttribute;
				if (wg != null)
					return wg.Info;
				var wi = ob as WebInvokeAttribute;
				if (wi != null)
					return wi.Info;
			}
			return new WebGetAttribute ().Info; // blank one
		}
	}

	public class WebHttpBehavior : IEndpointBehavior
	{
		public WebHttpBehavior ()
		{
			DefaultBodyStyle = WebMessageBodyStyle.Bare;
			DefaultOutgoingRequestFormat = WebMessageFormat.Xml;
			DefaultOutgoingResponseFormat = WebMessageFormat.Xml;
		}

		public virtual bool AutomaticFormatSelectionEnabled { get; set; }

		public virtual bool FaultExceptionEnabled { get; set; }

		public virtual bool HelpEnabled { get; set; }

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

#if !MOBILE && !XAMMAC_4_5
		protected virtual void AddServerErrorHandlers (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add (new WebHttpErrorHandler ());
		}
#endif

		public virtual void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			AddClientErrorInspector (endpoint, clientRuntime);
			foreach (ClientOperation oper in clientRuntime.Operations) {
				var req = GetRequestClientFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint);
				var res = GetReplyClientFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint);
				oper.Formatter = new ClientPairFormatter (req, res);
			}
		}

		public virtual void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
#if MOBILE || XAMMAC_4_5
			throw new NotImplementedException ();
#else
			endpointDispatcher.DispatchRuntime.OperationSelector = GetOperationSelector (endpoint);
			// FIXME: get HostNameComparisonMode from WebHttpBinding by some means.
			endpointDispatcher.FilterPriority = 1; // It is to take higher priority than that of ServiceMetadataExtension (whose URL likely conflicts with this one).
			endpointDispatcher.AddressFilter = new PrefixEndpointAddressMessageFilter (endpoint.Address);
			endpointDispatcher.ContractFilter = new MatchAllMessageFilter ();
			AddServerErrorHandlers (endpoint, endpointDispatcher);

			foreach (DispatchOperation oper in endpointDispatcher.DispatchRuntime.Operations) {
				var req = GetRequestDispatchFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint);
				var res = GetReplyDispatchFormatter (endpoint.Contract.Operations.Find (oper.Name), endpoint);
				oper.Formatter = new DispatchPairFormatter (req, res);
			}
			endpointDispatcher.DispatchRuntime.UnhandledDispatchOperation = new DispatchOperation (endpointDispatcher.DispatchRuntime, "*", "*", "*") {
				Invoker = new EndpointNotFoundOperationInvoker (),
				DeserializeRequest = false,
				SerializeReply = false};
#endif
		}

		internal class ClientPairFormatter : IClientMessageFormatter
		{
			public ClientPairFormatter (IClientMessageFormatter request, IClientMessageFormatter reply)
			{
				this.request = request;
				this.reply = reply;
			}

			IClientMessageFormatter request, reply;

			public Message SerializeRequest (MessageVersion messageVersion, object [] parameters)
			{
				return request.SerializeRequest (messageVersion, parameters);
			}

			public object DeserializeReply (Message message, object [] parameters)
			{
				return reply.DeserializeReply (message, parameters);
			}
		}

#if !MOBILE && !XAMMAC_4_5
		internal class DispatchPairFormatter : IDispatchMessageFormatter
		{
			public DispatchPairFormatter (IDispatchMessageFormatter request, IDispatchMessageFormatter reply)
			{
				this.request = request;
				this.reply = reply;
			}

			IDispatchMessageFormatter request;
			IDispatchMessageFormatter reply;

			public void DeserializeRequest (Message message, object [] parameters)
			{
				request.DeserializeRequest (message, parameters);
			}

			public Message SerializeReply (MessageVersion messageVersion, object [] parameters, object result)
			{
				return reply.SerializeReply (messageVersion, parameters, result);
			}
		}

		protected virtual WebHttpDispatchOperationSelector GetOperationSelector (ServiceEndpoint endpoint)
		{
			return new WebHttpDispatchOperationSelector (endpoint);
		}
#endif

		protected virtual QueryStringConverter GetQueryStringConverter (OperationDescription operationDescription)
		{
			return new QueryStringConverter ();
		}

		protected virtual IClientMessageFormatter GetReplyClientFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.ReplyClientFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}

#if !MOBILE
		protected virtual IDispatchMessageFormatter GetReplyDispatchFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.ReplyDispatchFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}
#endif

		protected virtual IClientMessageFormatter GetRequestClientFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.RequestClientFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}

#if !MOBILE
		protected virtual IDispatchMessageFormatter GetRequestDispatchFormatter (OperationDescription operationDescription, ServiceEndpoint endpoint)
		{
			return new WebMessageFormatter.RequestDispatchFormatter (operationDescription, endpoint, GetQueryStringConverter (operationDescription), this);
		}
#endif

		WebMessageBodyStyle GetBodyStyle (WebAttributeInfo wai)
		{
			return wai != null && wai.IsBodyStyleSetExplicitly ? wai.BodyStyle : DefaultBodyStyle;
		}

		protected void ValidateOperation (OperationDescription operation)
		{
			var wai = operation.GetWebAttributeInfo ();
			if (wai.Method == "GET")
				return;
			var style = GetBodyStyle (wai);

			// if the style is wrapped there won't be problems
			if (style == WebMessageBodyStyle.Wrapped)
				return;

			string [] parameters;
			if (wai.UriTemplate != null) {
				// find all variables in the URI
				var uri = new UriTemplate (wai.UriTemplate);
				parameters = new string [uri.PathSegmentVariableNames.Count + uri.QueryValueVariableNames.Count];
				uri.PathSegmentVariableNames.CopyTo (parameters, 0);
				uri.QueryValueVariableNames.CopyTo (parameters, uri.PathSegmentVariableNames.Count);

				// sort because Array.BinarySearch is the easiest way for case-insensitive search
				Array.Sort (parameters, StringComparer.InvariantCultureIgnoreCase);
			} else
				parameters = new string [0];

			bool hasBody = false;

			foreach (var msg in operation.Messages) {
				if (msg.Direction == MessageDirection.Input) {
					// the message is for a request
					// if requests are wrapped there is nothing to check
					if (style == WebMessageBodyStyle.WrappedRequest)
						continue;

					foreach (var part in msg.Body.Parts) {
						if (Array.BinarySearch (parameters, part.Name, StringComparer.InvariantCultureIgnoreCase) < 0) {
							// this part of the message is not covered by a variable in the URI
							// so it must be passed in the body
							if (hasBody)
								throw new InvalidOperationException (String.Format ("Operation '{0}' has multiple message body parts. Add parameters to the UriTemplate or change the BodyStyle to 'Wrapped' or 'WrappedRequest' on the WebInvoke/WebGet attribute.", operation.Name));
							hasBody = true;
						}
					}
				} else {
					// the message is for a response
					if (style != WebMessageBodyStyle.WrappedResponse && msg.Body.Parts.Count > 0)
						throw new InvalidOperationException (String.Format ("Operation '{0}' has output parameters. BodyStyle must be 'Wrapped' or 'WrappedResponse' on the operation WebInvoke/WebGet attribute.", operation.Name));
				}
			}
		}
		
		[MonoTODO ("check UriTemplate validity")]
		public virtual void Validate (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");

			foreach (var oper in endpoint.Contract.Operations) {
				ValidateOperation (oper);
			}

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

#if !MOBILE
		internal class WebHttpErrorHandler : IErrorHandler
		{
			public void ProvideFault (Exception error, MessageVersion version, ref Message fault)
			{
				if (!(error is EndpointNotFoundException))
					return;
				fault = Message.CreateMessage (version, null);
				var prop = new HttpResponseMessageProperty ();
				prop.StatusCode = HttpStatusCode.NotFound;
				fault.Properties.Add (HttpResponseMessageProperty.Name, prop);
			}
			
			public bool HandleError (Exception error)
			{
				return false;
			}
		}

		class EndpointNotFoundOperationInvoker : IOperationInvoker
		{
			public bool IsSynchronous {
				get { return true; }
			}

			public object [] AllocateInputs ()
			{
				return new object [1];
			}
			
			public object Invoke (object instance, object [] inputs, out object [] outputs)
			{
				throw new EndpointNotFoundException ();
			}
			
			public IAsyncResult InvokeBegin (object instance, object [] inputs, AsyncCallback callback, object state)
			{
				throw new EndpointNotFoundException ();
			}

			public object InvokeEnd (object instance, out object [] outputs, IAsyncResult result)
			{
				throw new InvalidOperationException ();
			}
		}
#endif
	}
}
