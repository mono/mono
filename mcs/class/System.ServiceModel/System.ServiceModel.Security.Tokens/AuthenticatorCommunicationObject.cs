//
// AuthenticatorCommunicationObject.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Security.Tokens
{
	abstract class AuthenticatorCommunicationObject : CommunicationObject
	{
		public abstract Message ProcessNegotiation (Message request, TimeSpan timeout);

		Binding issuer_binding;
		EndpointAddress issuer_address;
		Uri listen_uri;
		KeyedByTypeCollection<IEndpointBehavior> behaviors =
			new KeyedByTypeCollection<IEndpointBehavior> ();
		SecurityTokenSerializer serializer;
		SecurityAlgorithmSuite algorithm;
		SecurityBindingElement element;

		public EndpointAddress IssuerAddress {
			get { return issuer_address; }
			set { issuer_address = value; }
		}

		public Uri ListenUri {
			get { return listen_uri; }
			set { listen_uri = value; }
		}

		public Binding IssuerBinding {
			get { return issuer_binding; }
			set { issuer_binding = value; }
		}

		public KeyedByTypeCollection<IEndpointBehavior> IssuerChannelBehaviors {
			get { return behaviors; }
		}

		public SecurityAlgorithmSuite SecurityAlgorithmSuite {
			get { return algorithm; }
			set { algorithm= value; }
		}

		public SecurityBindingElement SecurityBindingElement {
			get { return element; }
			set { element = value; }
		}

		public SecurityTokenSerializer SecurityTokenSerializer {
			get { return serializer; }
			set { serializer = value; }
		}

		protected void EnsureProperties ()
		{
			if (State == CommunicationState.Opened)
				throw new InvalidOperationException ("Already opened.");

			if (SecurityTokenSerializer == null)
				throw new InvalidOperationException ("Security token serializer must be set before opening the token provider.");

			if (IssuerAddress == null)
				throw new InvalidOperationException ("Issuer address must be set before opening the token provider.");

			if (IssuerBinding == null)
				throw new InvalidOperationException ("IssuerBinding must be set before opening the token provider.");

			if (SecurityAlgorithmSuite == null)
				throw new InvalidOperationException ("Security algorithm suite must be set before opening the token provider.");

			if (ListenUri == null)
				throw new InvalidOperationException ("Listening uri must be set before opening the token provider.");
			if (SecurityBindingElement == null)
				throw new InvalidOperationException ("SecurityBindingElement must be set before opening the token provider.");
		}
	}
}
