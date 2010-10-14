//
// SecurityChannelFactory.cs
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
using System.ServiceModel.Channels.Security;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using System.Xml.XPath;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels
{
	internal class SecurityChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
	{
		IChannelFactory<TChannel> inner;
		InitiatorMessageSecurityBindingSupport security;

		public SecurityChannelFactory (
			IChannelFactory<TChannel> innerFactory, 
			InitiatorMessageSecurityBindingSupport security)
		{
			this.inner = innerFactory;
			this.security = security;
		}

		public InitiatorMessageSecurityBindingSupport SecuritySupport {
			get { return security; }
		}

		protected override TChannel OnCreateChannel (
			EndpointAddress remoteAddress, Uri via)
		{
			TChannel src = inner.CreateChannel (remoteAddress, via);

			if (typeof (TChannel) == typeof (IRequestChannel))
				return (TChannel) (object) new SecurityRequestChannel ((IRequestChannel) (object) src, (SecurityChannelFactory<IRequestChannel>) (object) this);
			if (typeof (TChannel) == typeof (IOutputChannel))
				return (TChannel) (object) new SecurityOutputChannel ((IOutputChannel) (object) src, (SecurityChannelFactory<IOutputChannel>) (object) this);
			if (typeof (TChannel) == typeof (IRequestSessionChannel))
				return (TChannel) (object) new SecurityRequestSessionChannel ((IRequestSessionChannel) (object) src, (SecurityChannelFactory<IRequestSessionChannel>) (object) this);
			if (typeof (TChannel) == typeof (IOutputSessionChannel))
				return (TChannel) (object) new SecurityOutputSessionChannel ((IOutputSessionChannel) (object) src, (SecurityChannelFactory<IOutputSessionChannel>) (object) this);

			throw new NotSupportedException (String.Format ("Channel type '{0}' is not supported", typeof (TChannel)));
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			inner.Open (timeout);
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			return inner.BeginOpen (timeout, callback, state);
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			inner.EndOpen (result);
		}
	}
}
