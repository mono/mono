//
// SecurityCapabilities.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2007 Novell, Inc.  http://www.novell.com
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
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;

using ReqType = System.ServiceModel.Security.Tokens.ServiceModelSecurityTokenRequirement;

namespace System.ServiceModel.Channels.Security
{
	internal abstract class SecurityCapabilities
		: ISecurityCapabilities
	{
		public abstract SecurityBindingElement Element { get; }

		public abstract bool AllowSerializedSigningTokenOnReply { get; }

		public abstract MessageProtectionOrder MessageProtectionOrder { get; }

		public abstract SecurityTokenParameters InitiatorParameters { get; }

		public abstract SecurityTokenParameters RecipientParameters { get; }

		public abstract bool RequireSignatureConfirmation { get; }

		public abstract string DefaultKeyWrapAlgorithm { get; }

		public abstract string DefaultSignatureAlgorithm { get; }


		// ISecurityCapabilities
		// FIXME: implement correctly
		public ProtectionLevel SupportedRequestProtectionLevel {
			get { return ProtectionLevel.EncryptAndSign; }
		}

		public ProtectionLevel SupportedResponseProtectionLevel {
			get { return ProtectionLevel.EncryptAndSign; }
		}

		public bool SupportsClientAuthentication {
			get { return InitiatorParameters != null ? InitiatorParameters.InternalSupportsClientAuthentication : false; }
		}

		public bool SupportsClientWindowsIdentity {
			get { return InitiatorParameters != null ? InitiatorParameters.InternalSupportsClientWindowsIdentity : false; }
		}

		public bool SupportsServerAuthentication {
			get { return RecipientParameters != null ? RecipientParameters.InternalSupportsServerAuthentication : false; }
		}
	}

	internal class SymmetricSecurityCapabilities : SecurityCapabilities
	{
		SymmetricSecurityBindingElement element;

		public SymmetricSecurityCapabilities (
			SymmetricSecurityBindingElement element)
		{
			this.element = element;
		}

		public override SecurityBindingElement Element {
			get { return element; }
		}

		// FIXME: const true or false
		public override bool AllowSerializedSigningTokenOnReply {
			get { throw new NotImplementedException (); }
		}

		public override MessageProtectionOrder MessageProtectionOrder {
			get { return element.MessageProtectionOrder; }
		}

		public override SecurityTokenParameters InitiatorParameters {
			get { return element.ProtectionTokenParameters; }
		}

		public override SecurityTokenParameters RecipientParameters {
			get { return element.ProtectionTokenParameters; }
		}

		public override bool RequireSignatureConfirmation {
			get { return element.RequireSignatureConfirmation; }
		}

		public override string DefaultSignatureAlgorithm {
			get { return element.DefaultAlgorithmSuite.DefaultSymmetricSignatureAlgorithm; }
		}

		public override string DefaultKeyWrapAlgorithm {
			get { return element.DefaultAlgorithmSuite.DefaultSymmetricKeyWrapAlgorithm; }
		}
	}

	internal class AsymmetricSecurityCapabilities : SecurityCapabilities
	{
		AsymmetricSecurityBindingElement element;

		public AsymmetricSecurityCapabilities (
			AsymmetricSecurityBindingElement element)
		{
			this.element = element;
		}

		public override bool AllowSerializedSigningTokenOnReply {
			get { return element.AllowSerializedSigningTokenOnReply; }
		}

		public override SecurityBindingElement Element {
			get { return element; }
		}

		public override MessageProtectionOrder MessageProtectionOrder {
			get { return element.MessageProtectionOrder; }
		}

		public override SecurityTokenParameters InitiatorParameters {
			get { return element.InitiatorTokenParameters; }
		}

		public override SecurityTokenParameters RecipientParameters {
			get { return element.RecipientTokenParameters; }
		}

		public override bool RequireSignatureConfirmation {
			get { return element.RequireSignatureConfirmation; }
		}

		public override string DefaultSignatureAlgorithm {
			get { return element.DefaultAlgorithmSuite.DefaultAsymmetricSignatureAlgorithm; }
		}

		public override string DefaultKeyWrapAlgorithm {
			get { return element.DefaultAlgorithmSuite.DefaultAsymmetricKeyWrapAlgorithm; }
		}
	}
}
