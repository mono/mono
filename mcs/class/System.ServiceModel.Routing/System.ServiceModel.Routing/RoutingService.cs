using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing
{
	[ServiceBehavior (AddressFilterMode = AddressFilterMode.Any, InstanceContextMode = InstanceContextMode.PerSession, UseSynchronizationContext = false, ValidateMustUnderstand = false)]
	public sealed class RoutingService : ISimplexDatagramRouter, ISimplexSessionRouter, IRequestReplyRouter, IDuplexSessionRouter
	{
/*
		class SimplexDatagramClient : ClientBase<ISimplexDatagramRouter>, ISimplexDatagramRouter
		{
			public IAsyncResult BeginProcessMessage (Message message, AsyncCallback callback, object state)
			{
				return Channel.BeginProcessMessage (message, callback, state);
			}

			public void EndProcessMessage (IAsyncResult result);
			{
				Channel.EndProcessMessage (result);
			}
		}

		class SimplexSessionClient : ClientBase<ISimplexSessionRouter>, ISimplexSessionRouter
		{
			public IAsyncResult BeginProcessMessage (Message message, AsyncCallback callback, object state)
			{
				return Channel.BeginProcessMessage (message, callback, state);
			}

			public void EndProcessMessage (IAsyncResult result);
			{
				Channel.EndProcessMessage (result);
			}
		}

		class DuplexSessionClient : ClientBase<IDuplexSessionRouter>, IDuplexSessionRouter
		{
			public IAsyncResult BeginProcessMessage (Message message, AsyncCallback callback, object state)
			{
				return Channel.BeginProcessMessage (message, callback, state);
			}

			public void EndProcessMessage (IAsyncResult result);
			{
				Channel.EndProcessMessage (result);
			}
		}

		class RequestReplyClient : ClientBase<IRequestReplyRouter>, IRequestReplyRouter
		{
			public IAsyncResult BeginProcessRequest (Message message, AsyncCallback callback, object state)
			{
				return Channel.BeginProcessRequest (message, callback, state);
			}

			public Message EndProcessRequest (IAsyncResult result);
			{
				return Channel.EndProcessRequest (result);
			}
		}
*/

		internal RoutingService ()
		{
		}

		internal RoutingConfiguration Configuration { get; set; }

		Action<Message> process_message_duplex_session_handler;
		Action<Message> process_message_simplex_datagram_handler;
		Action<Message> process_message_simplex_session_handler;
		//Func<Message,Message> process_request_handler;

		Dictionary<ServiceEndpoint,ChannelFactory> factories = new Dictionary<ServiceEndpoint,ChannelFactory> ();
		ChannelFactory<IRequestReplyRouter> request_reply_factory;
		IRequestReplyRouter request_reply_channel;
		//Dictionary<ServiceEndpoint,IChannel> sessions = new Dictionary<ServiceEndpoint,IChannel> ();

		IEnumerable<ServiceEndpoint> GetMatchingEndpoints (Message message)
		{
			IEnumerable<ServiceEndpoint> ret;
			if (!Configuration.FilterTable.GetMatchingValue (message, out ret))
				throw new EndpointNotFoundException ();
			return ret;
		}

//		static readonly MethodInfo create_factory_method = typeof (ChannelFactory).GetMethod ("CreateFactory", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

		void ProcessMessageDuplexSession (Message message)
		{
			var sel = GetMatchingEndpoints (message);
			foreach (var se in sel) {
				ChannelFactory cf;
				if (!factories.TryGetValue (se, out cf)) {
					cf = new ChannelFactory<IDuplexSessionRouter> (se);
					factories [se] = cf;
				}
				// FIXME: possibly reuse session channels, though I doubt saving session *at the router* makes sense...
				var ch = ((ChannelFactory<IDuplexSessionRouter>) cf).CreateChannel ();
				ch.EndProcessMessage (ch.BeginProcessMessage (message, null, null));
			}
		}

		void ProcessMessageSimplexDatagram (Message message)
		{
			var sel = GetMatchingEndpoints (message);
			foreach (var se in sel) {
				ChannelFactory cf;
				if (!factories.TryGetValue (se, out cf)) {
					cf = new ChannelFactory<ISimplexDatagramRouter> (se);
					factories [se] = cf;
				}
				var ch = ((ChannelFactory<ISimplexDatagramRouter>) cf).CreateChannel ();
				ch.EndProcessMessage (ch.BeginProcessMessage (message, null, null));
			}
		}

		void ProcessMessageSimplexSession (Message message)
		{
			var sel = GetMatchingEndpoints (message);
			foreach (var se in sel) {
				ChannelFactory cf;
				if (!factories.TryGetValue (se, out cf)) {
					cf = new ChannelFactory<ISimplexSessionRouter> (se);
					factories [se] = cf;
				}
				// FIXME: possibly reuse session channels, though I doubt saving session *at the router* makes sense...
				var ch = ((ChannelFactory<ISimplexSessionRouter>) cf).CreateChannel ();
				ch.EndProcessMessage (ch.BeginProcessMessage (message, null, null));
			}
		}

		IAsyncResult IDuplexSessionRouter.BeginProcessMessage (Message message, AsyncCallback callback, object state)
		{
			if (process_message_duplex_session_handler == null)
				process_message_duplex_session_handler = new Action<Message> (ProcessMessageDuplexSession);
			return process_message_duplex_session_handler.BeginInvoke (message, callback, state);
		}

		void IDuplexSessionRouter.EndProcessMessage (IAsyncResult result)
		{
			if (process_message_duplex_session_handler == null)
				throw new InvalidOperationException ("Async operation has not started");
			process_message_duplex_session_handler.EndInvoke (result);
		}

		IAsyncResult IRequestReplyRouter.BeginProcessRequest (Message message, AsyncCallback callback, object state)
		{
			if (request_reply_channel != null)
				throw new InvalidOperationException ("Another async request operation is in progress");

			var sel = GetMatchingEndpoints (message);
			ServiceEndpoint se = null;
			foreach (var se_ in sel) {
				if (se != null)
					throw new InvalidOperationException ("Multiple endpoints cannot be specified for request-reply channel");
				se = se_;
			}
			if (se == null)
				throw new InvalidOperationException ("No service endpoint is registered to the request-reply channel");

			if (request_reply_factory == null)
				request_reply_factory = new ChannelFactory<IRequestReplyRouter> (se);
			request_reply_channel = request_reply_factory.CreateChannel ();
			return request_reply_channel.BeginProcessRequest (message, null, null);
		}

		Message IRequestReplyRouter.EndProcessRequest (IAsyncResult result)
		{
			if (request_reply_channel == null)
				throw new InvalidOperationException ("Async request has not started");
			var ch = request_reply_channel;
			request_reply_channel = null;
			return ch.EndProcessRequest (result);
		}

		IAsyncResult ISimplexDatagramRouter.BeginProcessMessage (Message message, AsyncCallback callback, object state)
		{
			if (process_message_simplex_datagram_handler == null)
				process_message_simplex_datagram_handler = new Action<Message> (ProcessMessageSimplexDatagram);
			return process_message_simplex_datagram_handler.BeginInvoke (message, callback, state);
		}

		void ISimplexDatagramRouter.EndProcessMessage (IAsyncResult result)
		{
			if (process_message_simplex_datagram_handler == null)
				throw new InvalidOperationException ("Async operation has not started");
			process_message_simplex_datagram_handler.EndInvoke (result);
		}

		IAsyncResult ISimplexSessionRouter.BeginProcessMessage (Message message, AsyncCallback callback, object state)
		{
			if (process_message_simplex_session_handler == null)
				process_message_simplex_session_handler = new Action<Message> (ProcessMessageSimplexSession);
			return process_message_simplex_session_handler.BeginInvoke (message, callback, state);
		}

		void ISimplexSessionRouter.EndProcessMessage (IAsyncResult result)
		{
			if (process_message_simplex_session_handler == null)
				throw new InvalidOperationException ("Async operation has not started");
			process_message_simplex_session_handler.EndInvoke (result);
		}
	}
}
