//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Discovery
{
	class InspectionBehavior : IEndpointBehavior
	{
		public static object State { get; set; }

		public event PostDispatchMessageReceiveHandler RequestReceived;
		public event PreDispatchMessageSendHandler ReplySending;

		public bool DispatchBehaviorApplied { get; set; }

		public void AddBindingParameters (ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}
		
		public void ApplyClientBehavior (ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}
		
		public void ApplyDispatchBehavior (ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			DispatchBehaviorApplied = true;

			var inspector = new DispatchMessageInspector ();
			inspector.RequestReceived += delegate (ref Message message, IClientChannel channel, InstanceContext instanceContext) {
				if (RequestReceived != null)
					return RequestReceived (ref message, channel, instanceContext);
				else
					return null;
			};
			inspector.ReplySending += delegate (ref Message reply, object correlationState) {
				if (ReplySending != null)
					ReplySending (ref reply, correlationState);
			};
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add (inspector);
		}
		
		public void Validate (ServiceEndpoint endpoint)
		{
		}
	}
	
	delegate object PostDispatchMessageReceiveHandler (ref Message request, IClientChannel channel, InstanceContext instanceContext);
	delegate void PreDispatchMessageSendHandler (ref Message request,object correleationState);

	class DispatchMessageInspector : IDispatchMessageInspector
	{
		public event PostDispatchMessageReceiveHandler RequestReceived;
		public event PreDispatchMessageSendHandler ReplySending;

		public object AfterReceiveRequest (ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			if (RequestReceived != null)
				return RequestReceived (ref request, channel, instanceContext);
			else
				return null;
		}

		public void BeforeSendReply (ref Message reply, object correlationState)
		{
			if (ReplySending != null)
				ReplySending (ref reply, correlationState);
		}
	}
}
