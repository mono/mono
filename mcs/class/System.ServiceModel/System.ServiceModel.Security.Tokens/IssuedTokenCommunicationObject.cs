//
// IssuedTokenCommunicationObject.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;

namespace System.ServiceModel.Security.Tokens
{
	class IssuedTokenCommunicationObject : ProviderCommunicationObject
	{
		WSTrustSecurityTokenServiceProxy comm;

		public SecurityToken GetToken (TimeSpan timeout)
		{
			WstRequestSecurityToken req = new WstRequestSecurityToken ();
			BodyWriter body = new WstRequestSecurityTokenWriter (req, SecurityTokenSerializer);
			Message msg = Message.CreateMessage (IssuerBinding.MessageVersion, Constants.WstIssueAction, body);
			Message res = comm.Issue (msg);

			// FIXME: provide SecurityTokenResolver (but from where?)
			using (WSTrustRequestSecurityTokenResponseReader resreader = new WSTrustRequestSecurityTokenResponseReader (null, res.GetReaderAtBodyContents (), SecurityTokenSerializer, null)) {
				WstRequestSecurityTokenResponse rstr = resreader.Read ();
				if (rstr.RequestedSecurityToken != null)
					return rstr.RequestedSecurityToken;
				throw new NotImplementedException ("IssuedSecurityTokenProvider did not see RequestedSecurityToken in the response.");
			}
		}

		protected internal override TimeSpan DefaultCloseTimeout {
			get { return comm == null ? DefaultCommunicationTimeouts.Instance.CloseTimeout : comm.ChannelFactory.DefaultCloseTimeout; }
		}

		protected internal override TimeSpan DefaultOpenTimeout {
			get { return comm == null ? DefaultCommunicationTimeouts.Instance.OpenTimeout : comm.ChannelFactory.DefaultOpenTimeout; }
		}

		protected override void OnAbort ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnOpen (TimeSpan timeout)
		{
			if (comm != null)
				throw new InvalidOperationException ("Already opened.");

			EnsureProperties ();

			comm = new WSTrustSecurityTokenServiceProxy (
				IssuerBinding, IssuerAddress);
			KeyedByTypeCollection<IEndpointBehavior> bl =
				comm.Endpoint.Behaviors;
			foreach (IEndpointBehavior b in IssuerChannelBehaviors) {
				bl.Remove (b.GetType ());
				bl.Add (b);
			}
			comm.Open ();
		}

		protected override IAsyncResult OnBeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		protected override void OnClose (TimeSpan timeout)
		{
			comm.Close ();
		}

		protected override IAsyncResult OnBeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}
	}
}
