using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing
{
	public class SoapProcessingBehavior : IEndpointBehavior
	{
		class MessageInspector : IClientMessageInspector
		{
			public MessageInspector (SoapProcessingBehavior owner, ServiceEndpoint endpoint)
			{
				this.endpoint = endpoint;
			}

			ServiceEndpoint endpoint;

			public object BeforeSendRequest (ref Message request, IClientChannel channel)
			{
				// See "Request processing" at http://msdn.microsoft.com/en-us/library/ee517422%28VS.100%29.aspx

				var original = request;
				var msg = Message.CreateMessage (endpoint.Binding.MessageVersion, request.Headers.Action, request.GetReaderAtBodyContents ());
				// The above page does not explain, but it should do copy headers (other than addressing ones, to be removed later).
				msg.Headers.Action = null;
				msg.Headers.CopyHeadersFrom (request);
				msg.Headers.To = null;
				msg.Headers.From = null;
				msg.Headers.FaultTo = null;
				msg.Headers.RelatesTo = null;
				msg.Properties.CopyProperties (request.Properties);

				request = msg;
				return original;
			}

			public void AfterReceiveReply (ref Message reply, object correlationState)
			{
				// See "Response processing" at http://msdn.microsoft.com/en-us/library/ee517422%28VS.100%29.aspx

				Message original = (Message) correlationState;

				var msg = Message.CreateMessage (original.Version, reply.Headers.Action, reply.GetReaderAtBodyContents ());
				// The above page does not explain, but it should do copy headers (other than addressing ones, to be removed later).
				msg.Headers.Action = null;
				msg.Headers.CopyHeadersFrom (reply);
				/*
				msg.Headers.To = null;
				msg.Headers.From = null;
				msg.Headers.FaultTo = null;
				msg.Headers.RelatesTo = null;
				*/
				msg.Properties.CopyProperties (reply.Properties);
				reply = msg;
			}
		}

		public SoapProcessingBehavior ()
		{
			ProcessMessages = true;
		}

		public bool ProcessMessages { get; set; }

		public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
			// nothing to do here.
		}

		public void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
			if (!ProcessMessages)
				return;
			clientRuntime.MessageInspectors.Add (new MessageInspector (this, endpoint));
		}

		public void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			throw new NotSupportedException (); // ... right? I assume it is not for services.
		}

		public void Validate (ServiceEndpoint endpoint)
		{
		}
	}
}
