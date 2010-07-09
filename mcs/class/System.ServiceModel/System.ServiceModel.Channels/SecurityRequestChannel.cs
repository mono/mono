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

namespace System.ServiceModel.Channels
{
	interface ISecurityChannelSource
	{
		MessageSecurityBindingSupport Support { get; }
	}

	class SecurityRequestChannel : SecurityRequestChannelBase
	{
		SecurityChannelFactory<IRequestChannel> source;

		public SecurityRequestChannel (IRequestChannel innerChannel, SecurityChannelFactory<IRequestChannel> source)
			: base (innerChannel)
		{
			this.source = source;
			InitializeSecurityFunctionality (source.SecuritySupport);
		}

		public override ChannelFactoryBase Factory {
			get { return source; }
		}
	}

	class SecurityRequestSessionChannel : SecurityRequestChannelBase
	{
		SecurityChannelFactory<IRequestSessionChannel> source;

		public SecurityRequestSessionChannel (IRequestSessionChannel innerChannel, SecurityChannelFactory<IRequestSessionChannel> source)
			: base (innerChannel)
		{
			this.source = source;
			InitializeSecurityFunctionality (source.SecuritySupport);
		}

		public override ChannelFactoryBase Factory {
			get { return source; }
		}
	}

	abstract class SecurityRequestChannelBase : LayeredRequestChannel
	{
		InitiatorMessageSecurityBindingSupport security;

		protected SecurityRequestChannelBase (IRequestChannel innerChannel)
			: base (innerChannel)
		{
			Opened += new EventHandler (AcquireSecurityKey);
			Closing += new EventHandler (ReleaseSecurityKey);
		}

		protected void InitializeSecurityFunctionality (InitiatorMessageSecurityBindingSupport security)
		{
			this.security = security;
		}

		SecurityMessageProperty secprop;

		protected override IAsyncResult OnBeginRequest (Message message, TimeSpan timeout, AsyncCallback callback, object state)
		{
			// FIXME: make it really async
			Message secure = SecureMessage (message);
			secprop = secure.Properties.Security;
			return base.BeginRequest (secure, timeout, callback, state);
		}

		protected override Message OnEndRequest (IAsyncResult result)
		{
			// FIXME: it must be also asynchronized.
			Message raw = base.EndRequest (result);
			return ProcessReply (raw, secprop);
		}

		protected override Message OnRequest (Message message, TimeSpan timeout)
		{
			Message secure = SecureMessage (message);
			Message raw = base.OnRequest (secure, timeout);
			return ProcessReply (raw, secure.Properties.Security);
		}

		Message SecureMessage (Message msg)
		{
			return new InitiatorMessageSecurityGenerator (msg, security, RemoteAddress).SecureMessage ();
		}

		Message ProcessReply (Message message, SecurityMessageProperty secprop)
		{
			// FIXME: provide correct parameters
			return message.IsFault ? message : new InitiatorSecureMessageDecryptor (message, secprop, security).DecryptMessage ();
		}

		void AcquireSecurityKey (object o, EventArgs e)
		{
			security.Prepare (Factory, RemoteAddress);
		}

		void ReleaseSecurityKey (object o, EventArgs e)
		{
			security.Release ();
		}
	}
}
