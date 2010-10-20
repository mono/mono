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
using System.Web.Script.Services;

namespace System.ServiceModel.Description
{
	[ServiceContract (Namespace = "")]
	internal class InteropScriptService
	{
		Type type;
		string path;
		bool debug;

		public InteropScriptService (Type type, string path, bool debug)
		{
			this.type = type;
			this.path = path;
			this.debug = debug;
		}

		[WebGet (UriTemplate = "*")]
		[OperationContract]
		public string Get ()
		{
			return ProxyGenerator.GetClientProxyScript (type, path, debug);
		}
	}

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
			base.AddClientErrorInspector (endpoint, clientRuntime);
		}

		[MonoTODO]
		protected override void AddServerErrorHandlers (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			base.AddServerErrorHandlers (endpoint, endpointDispatcher);
		}

		[MonoTODO]
		public override void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			base.ApplyClientBehavior (endpoint, clientRuntime);
		}

		public override void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			base.ApplyDispatchBehavior (endpoint, endpointDispatcher);

			// doing similar to ServiceMetadataExtension
			BuildScriptDispatcher (endpoint, endpointDispatcher, "js", false);
			BuildScriptDispatcher (endpoint, endpointDispatcher, "jsdebug", true);
		}

		void BuildScriptDispatcher (ServiceEndpoint endpoint, EndpointDispatcher ed, string subPath, bool debug)
		{
			var instance = new InteropScriptService (endpoint.Contract.ContractType, endpoint.Address.Uri.ToString (), debug);

			var cdOrg = ed.ChannelDispatcher;
			var baseUriString = endpoint.ListenUri.ToString ();
			var uri = new Uri (String.Concat (baseUriString, baseUriString [baseUriString.Length - 1] == '/' ? String.Empty : "/", subPath));
			var listener = endpoint.Binding.BuildChannelListener<IReplyChannel> (uri);
			var cd = new ChannelDispatcher (listener, String.Empty);

			cd.MessageVersion = MessageVersion.None;
			cd.Endpoints.Add (new EndpointDispatcher (new EndpointAddress (uri), "InteropScriptService", String.Empty)
				{ ContractFilter = new MatchAllMessageFilter () });

			var dr = cd.Endpoints [0].DispatchRuntime;
			var dop = new DispatchOperation (dr, "Get", "*", "*");
			dop.DeserializeRequest = false;
			dop.SerializeReply = false;
			dop.Invoker = new DummyInvoker (instance);
			dr.UnhandledDispatchOperation = dop;
			dr.InstanceContextProvider = new SingletonInstanceContextProvider (new InstanceContext (cdOrg.Host, instance));

			var host = ed.ChannelDispatcher.Host;
			host.ChannelDispatchers.Add (cd);
		}

		class DummyInvoker : IOperationInvoker
		{
			InteropScriptService instance;

			public DummyInvoker (InteropScriptService instance)
			{
				this.instance = instance;
			}

			public object [] AllocateInputs ()
			{
				return new object [0];
			}

			public object Invoke (object instance, object [] inputs, out object [] outputs)
			{
				outputs = new object [0];
				var msg = Message.CreateMessage (MessageVersion.None, "*", (object) null);
				var hp = new HttpResponseMessageProperty ();
				hp.Headers ["Content-Type"] = "text/javascript";
				msg.Properties.Add (HttpResponseMessageProperty.Name, hp);
				msg.Properties.Add (WebMessageEncoder.ScriptPropertyName, this.instance.Get ());
				return msg;
			}

			public IAsyncResult InvokeBegin (object instance, object[] inputs, AsyncCallback callback, object state)
			{
				throw new NotSupportedException ();
			}

			public object InvokeEnd (object instance, out object [] outputs, IAsyncResult result)
			{
				throw new NotSupportedException ();
			}

			public bool IsSynchronous {
				get { return true; }
			}
		}

		protected override QueryStringConverter GetQueryStringConverter (OperationDescription operationDescription)
		{
			return new JsonQueryStringConverter () { CustomWrapperName = "d"};
		}

		[MonoTODO ("add non-XmlSerializer-ness check (but where?)")]
		public override void Validate (ServiceEndpoint endpoint)
		{
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");
			ValidateBinding (endpoint);

			foreach (var od in endpoint.Contract.Operations) {
				var wai = od.GetWebAttributeInfo ();
				if (wai.UriTemplate != null)
					throw new InvalidOperationException ("UriTemplate must not be used with WebScriptEnablingBehavior");
				var wia = od.Behaviors.Find<WebInvokeAttribute> ();
				if (wia != null) {
					switch (wia.Method.ToUpper ()) {
					case "GET":
					case "POST":
						break;
					default:
						throw new InvalidOperationException ("Only GET and POST HTTP methods are valid used for WebScriptEnablingBehavior");
					}
				}

				var style = wai != null && wai.IsBodyStyleSetExplicitly ? wai.BodyStyle : DefaultBodyStyle;
				if (style != WebMessageBodyStyle.WrappedRequest)
					throw new NotSupportedException (String.Format ("WebScriptEnableBehavior only allows WrappedRequest body style, but operation '{0}' uses {1}.", od.Name, style));
			}
		}
	}
}
