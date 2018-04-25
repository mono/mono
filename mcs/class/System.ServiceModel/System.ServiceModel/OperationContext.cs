//
// OperationContext.cs
//
// Author: Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2005,2007 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Threading;

namespace System.ServiceModel
{
	public sealed class OperationContext : IExtensibleObject<OperationContext>
	{
		[ThreadStatic]
		static OperationContext current;

		public static OperationContext Current {
			get { return current; }
			set { current = value; }
		}

		Message incoming_message;
#if !MOBILE && !XAMMAC_4_5
		EndpointDispatcher dispatcher;
#endif
		IContextChannel channel;
		RequestContext request_ctx;
		ExtensionCollection<OperationContext> extensions;
		MessageHeaders outgoing_headers;
		MessageProperties outgoing_properties;
		InstanceContext instance_context;

		public OperationContext (IContextChannel channel)
			: this (channel, true)
		{
		}

		internal OperationContext (IContextChannel channel, bool isUserContext)
		{
			if (channel == null)
				throw new ArgumentNullException ("channel");
			this.channel = channel;
			IsUserContext = isUserContext;
		}

		public event EventHandler OperationCompleted;

		public IContextChannel Channel {
			get { return channel; }
		}

		public IExtensionCollection<OperationContext> Extensions {
			get {
				if (extensions == null)
					extensions = new ExtensionCollection<OperationContext> (this);
				return extensions;
			}
		}


#if !MOBILE && !XAMMAC_4_5
		public EndpointDispatcher EndpointDispatcher {
			get { return dispatcher; }
			set { dispatcher = value; }
		}
		public bool HasSupportingTokens {
			get { return SupportingTokens != null ? SupportingTokens.Count > 0 : false; }
		}

		public ServiceHostBase Host {
			get { return dispatcher != null ? dispatcher.ChannelDispatcher.Host : null; }
		}
#endif

		public MessageHeaders IncomingMessageHeaders {
			get { return incoming_message != null ? incoming_message.Headers : null; }
		}

		public MessageProperties IncomingMessageProperties {
			get { return incoming_message != null ? incoming_message.Properties : null; }
		}

		public MessageVersion IncomingMessageVersion {
			get { return incoming_message != null ? incoming_message.Version : null; }
		}

		[MonoTODO]
		public InstanceContext InstanceContext {
			get {				
				return instance_context;
			}
			internal set {
				instance_context = value;
			}
		}

		public bool IsUserContext { get; private set; }

		public MessageHeaders OutgoingMessageHeaders {
			get {
				if (outgoing_headers == null)
					outgoing_headers = new MessageHeaders (channel.GetProperty<MessageVersion> () ?? MessageVersion.Default);
				return outgoing_headers;
			}
		}

		public MessageProperties OutgoingMessageProperties {
			get {
				if (outgoing_properties == null)
					outgoing_properties = new MessageProperties ();
				return outgoing_properties;
			}
		}

		public RequestContext RequestContext {
			get { return request_ctx; }
			set { request_ctx = value; }
		}

		public string SessionId {
			get { return Channel.SessionId; }
		}

#if !MOBILE
		public ServiceSecurityContext ServiceSecurityContext {
			get { return IncomingMessageProperties != null ? IncomingMessageProperties.Security.ServiceSecurityContext : null; }
		}

		public ICollection<SupportingTokenSpecification> SupportingTokens {
			get { return IncomingMessageProperties != null ? IncomingMessageProperties.Security.IncomingSupportingTokens : null; }
		}

		public T GetCallbackChannel<T> ()
		{
			// It is correct; OperationContext.Channel and OperationContext.GetCallbackChannel<T>() returns the same instance on .NET. (at least as far as I tested.)
			return (T) (object) channel;
		}

		[MonoTODO]
		public void SetTransactionComplete ()
		{
			throw new NotImplementedException ();
		}
#endif

		internal Message IncomingMessage {
			get {
				return incoming_message;
			}
			set {
				incoming_message = value;
			}
		}
	}
}
