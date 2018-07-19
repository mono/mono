//
// X509CertificateValidator.cs
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
using System;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;

namespace System.IdentityModel.Selectors
{
	public abstract class X509CertificateValidator
	{
		static X509CertificateValidator none, chain, peer_or_chain, peer;

		static X509CertificateValidator ()
		{
			none = new X509NoValidator ();
			chain = new X509CertificateValidatorImpl (
				false, true, false, new X509ChainPolicy ());
			peer = new X509CertificateValidatorImpl (
				true, false, false, null);
			peer_or_chain = new X509CertificateValidatorImpl (
				true, true, false, new X509ChainPolicy ());
		}

		protected X509CertificateValidator ()
		{
		}

		public static X509CertificateValidator None {
			get { return none; }
		}

		public static X509CertificateValidator ChainTrust {
			get { return chain; }
		}

		public static X509CertificateValidator PeerOrChainTrust {
			get { return peer_or_chain; }
		}

		public static X509CertificateValidator PeerTrust {
			get { return peer; }
		}

		public static X509CertificateValidator CreateChainTrustValidator (
			bool useMachineContext, X509ChainPolicy chainPolicy)
		{
			return new X509CertificateValidatorImpl (
				false, true, useMachineContext, chainPolicy);
		}

		public static X509CertificateValidator CreatePeerOrChainTrustValidator (
			bool useMachineContext, X509ChainPolicy chainPolicy)
		{
			return new X509CertificateValidatorImpl (
				true, true, useMachineContext, chainPolicy);
		}

		public abstract void Validate (X509Certificate2 certificate);

		class X509NoValidator : X509CertificateValidator
		{
			public override void Validate (X509Certificate2 cert)
			{
			}
		}

		class X509CertificateValidatorImpl : X509CertificateValidator
		{
			bool check_peer;
			bool check_chain;
			bool use_machine_ctx;
			X509ChainPolicy policy;
			X509Chain chain;

			public X509CertificateValidatorImpl (bool peer, bool chain, bool useMachineContext, X509ChainPolicy chainPolicy)
			{
				this.check_peer = peer;
				this.check_chain = chain;
				use_machine_ctx = useMachineContext;
				policy = chainPolicy;
			}

			public override void Validate (X509Certificate2 cert)
			{
				if (check_peer) {
					X509Store store = new X509Store ();
					store.Open (OpenFlags.ReadOnly);
					foreach (X509Certificate2 c in store.Certificates)
						if (c.Thumbprint == cert.Thumbprint)
							return;
				}
				if (check_chain) {
					if (chain == null) {
						if (use_machine_ctx)
							chain = X509Chain.Create ();
						else
							chain = new X509Chain ();
						chain.ChainPolicy = policy;
					}
					else
						chain.Reset ();
					if (chain.Build (cert))
						return;
				}
				throw new ArgumentException ("The argument certificate is invalid.");
			}
		}
	}
}
