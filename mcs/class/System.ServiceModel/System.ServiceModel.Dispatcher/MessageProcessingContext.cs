using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Dispatcher 
{
	internal class MessageProcessingContext 
	{
		OperationContext operation_context;
		RequestContext request_context;
		Message incoming_message;

		Message reply_message;		
		InstanceContext instance_context;		
		Exception processingException;
		DispatchOperation operation;
		UserEventsHandler user_events_handler;		

		public MessageProcessingContext (OperationContext opCtx)
		{
			operation_context = opCtx;
			request_context = opCtx.RequestContext;
			incoming_message = opCtx.IncomingMessage;
			user_events_handler = new UserEventsHandler (this);
		}

		public DispatchOperation Operation
		{
			get { return operation; }
			set { operation = value; }
		}

		public Exception ProcessingException
		{
			get { return processingException; }
			set { processingException = value; }
		}
		
		public Message ReplyMessage
		{
			get { return reply_message; }
			set { reply_message = value; }
		}

		public InstanceContext InstanceContext
		{
			get { return instance_context; }
			set { instance_context = value; }
		}

		public Message IncomingMessage
		{
			get { return incoming_message; }
			set { incoming_message = value; }
		}

		public RequestContext RequestContext
		{
			get { return request_context; }
			set { request_context = value; }
		}

		public OperationContext OperationContext
		{
			get { return operation_context; }
			set { operation_context = value; }
		}

		public UserEventsHandler EventsHandler
		{
			get { return user_events_handler; }
			set { user_events_handler = value; }
		}

		public void Reply (IDuplexChannel channel, bool useTimeout)
		{
			EventsHandler.BeforeSendReply ();
			if (useTimeout && Operation.Parent.ChannelDispatcher != null) // FIXME: this condition is a workaround for NRE, there might be better way to get timeout value.
				channel.Send (ReplyMessage, Operation.Parent.ChannelDispatcher.timeouts.SendTimeout);
			else
				channel.Send (ReplyMessage);
		}

		public void Reply (bool useTimeout)
		{
			EventsHandler.BeforeSendReply ();
			if (useTimeout && Operation.Parent.ChannelDispatcher != null) // FIXME: this condition is a workaround for NRE, there might be better way to get timeout value.
				RequestContext.Reply (ReplyMessage, Operation.Parent.ChannelDispatcher.timeouts.SendTimeout);
			else
				RequestContext.Reply (ReplyMessage);
		}
	}

	#region user events implementation

	internal class UserEventsHandler
	{
		MessageProcessingContext request_context;
		DispatchRuntime dispatch_runtime;
		IClientChannel channel;
		object [] msg_inspectors_states;
		object [] callcontext_initializers_states;

		public UserEventsHandler (MessageProcessingContext mrc)
		{
			request_context = mrc;
			dispatch_runtime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			msg_inspectors_states = new object [dispatch_runtime.MessageInspectors.Count];
			channel = request_context.OperationContext.Channel as IClientChannel;
		}

		public void AfterReceiveRequest ()
		{
			Message message = request_context.IncomingMessage;

			for (int i = 0; i < dispatch_runtime.MessageInspectors.Count; ++i)
				msg_inspectors_states [i] = dispatch_runtime.MessageInspectors [i].AfterReceiveRequest (
					   ref message, channel, request_context.InstanceContext);
			request_context.IncomingMessage = message;

		}

		public void BeforeSendReply ()
		{
			Message toBeChanged = request_context.ReplyMessage;
			for (int i = 0; i < dispatch_runtime.MessageInspectors.Count; ++i)
				dispatch_runtime.MessageInspectors [i].BeforeSendReply (ref toBeChanged, msg_inspectors_states [i]);
		}

		public void BeforeInvoke (DispatchOperation operation)
		{
			callcontext_initializers_states = new object [operation.CallContextInitializers.Count];
			for (int i = 0; i < callcontext_initializers_states.Length; ++i)
				callcontext_initializers_states [i] = operation.CallContextInitializers [i].BeforeInvoke (
					request_context.InstanceContext, channel, request_context.IncomingMessage);

		}

		public void AfterInvoke (DispatchOperation operation)
		{
			for (int i = 0; i < callcontext_initializers_states.Length; ++i)
				operation.CallContextInitializers [i].AfterInvoke (callcontext_initializers_states [i]);
		}
	}

	#endregion
}
