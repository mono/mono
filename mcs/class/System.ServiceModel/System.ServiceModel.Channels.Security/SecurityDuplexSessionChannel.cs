//
// SecurityRequestChannel.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using System.Xml.XPath;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels.Security
{
	class SecurityDuplexSession : DuplexSessionBase
	{
		SecurityDuplexSessionChannel channel;
		
		public SecurityDuplexSession (SecurityDuplexSessionChannel channel)
		{
			this.channel = channel;
		}
		
		public override TimeSpan DefaultCloseTimeout {
			get { return channel.DefaultCloseTimeout; }
		}
		
		public override void Close (TimeSpan timeout)
		{
			// valid only if the inner channel is ISessionChannel
			var d = channel.Channel as IDuplexSessionChannel;
			if (d != null)
				d.Session.CloseOutputSession (timeout);
		}
	}
	
	class SecurityDuplexSessionChannel : DuplexChannelBase, IDuplexSessionChannel
	{
		IChannel channel;
		InitiatorMessageSecurityBindingSupport security_initiator;
		RecipientMessageSecurityBindingSupport security_recipient;
		SecurityDuplexSession session;
		
		public SecurityDuplexSessionChannel (ChannelFactoryBase factory, IChannel innerChannel, EndpointAddress remoteAddress, Uri via, InitiatorMessageSecurityBindingSupport security)
			: base (factory, remoteAddress, via)
		{
			this.channel = innerChannel;
			session = new SecurityDuplexSession (this);
			InitializeSecurityFunctionality (security);
		}
		
		public SecurityDuplexSessionChannel (ChannelListenerBase listener, IChannel innerChannel, RecipientMessageSecurityBindingSupport security)
			: base (listener)
		{
			this.channel = innerChannel;
			session = new SecurityDuplexSession (this);
			InitializeSecurityFunctionality (security);
		}
		
		public IChannel Channel {
			get { return channel; }
		}

		public IDuplexSession Session {
			get { return session; }
		}

		void InitializeSecurityFunctionality (InitiatorMessageSecurityBindingSupport security)
		{
			security_initiator = security;
		}

		void InitializeSecurityFunctionality (RecipientMessageSecurityBindingSupport security)
		{
			security_recipient = security;
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			channel.Open (timeout);
			if (security_initiator != null)
				security_initiator.Prepare ((ChannelFactoryBase) Manager, RemoteAddress);
			else
				security_recipient.Prepare ((ChannelListenerBase) Manager, LocalAddress.Uri);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			if (security_initiator != null)
				security_initiator.Release ();
			else
				security_recipient.Release ();
			channel.Close (timeout);
		}

		protected override void OnAbort ()
		{
			if (security_initiator != null)
				security_initiator.Release ();
			else
				security_recipient.Release ();
			channel.Abort ();
		}

		public override bool TryReceive (TimeSpan timeout, out Message message)
		{
			ThrowIfDisposedOrNotOpen ();
			var input = (IInputChannel) channel;
			if (!input.TryReceive (timeout, out message))
				return false;
			message = DecryptMessage (message);
			return true;
		}

		public override bool WaitForMessage (TimeSpan timeout)
		{
			var input = (IInputChannel) channel;
			return input.WaitForMessage (timeout);
		}

		public override void Send (Message message)
		{
			Send (message, DefaultSendTimeout);
		}

		public override void Send (Message message, TimeSpan timeout)
		{
			Message secure = SecureMessage (message);
			var output = (IOutputChannel) channel;
			output.Send (secure, timeout);
		}

		Message SecureMessage (Message msg)
		{
			if (security_initiator != null)
				return new InitiatorMessageSecurityGenerator (msg, security_initiator, RemoteAddress).SecureMessage ();
			else
				return new RecipientMessageSecurityGenerator (msg, null, security_recipient).SecureMessage (); // FIXME: supply SecurityMessageProperty (if any)
		}

		Message DecryptMessage (Message msg)
		{
			if (security_initiator != null)
				return new InitiatorSecureMessageDecryptor (msg, null, security_initiator).DecryptMessage (); // FIXME: supply SecurityMessageProperty (if any)
			else
				return new RecipientSecureMessageDecryptor (msg, security_recipient).DecryptMessage ();
		}
	}
}

