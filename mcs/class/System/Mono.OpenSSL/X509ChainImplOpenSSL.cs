//
// X509ChainImplOpenSSL.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if SECURITY_DEP && MONO_FEATURE_OPENSSL
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MX = MonoSecurity::Mono.Security.X509;
#else
using MX = Mono.Security.X509;
#endif
using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.OpenSSL
{
	class X509ChainImplOpenSSL : X509ChainImpl
	{
		MonoOpenSSLX509StoreCtx storeCtx;
		MonoOpenSSLX509Chain chain;
		MonoOpenSSLX509Chain untrustedChain;
		X509ChainElementCollection elements;
		X509Certificate2Collection untrusted;
		X509Certificate2[] certificates;
		X509ChainPolicy policy;
		List<X509ChainStatus> chainStatusList;

		internal X509ChainImplOpenSSL (MonoOpenSSLX509Chain chain)
		{
			this.chain = chain.Copy ();
			policy = new X509ChainPolicy ();
		}

		internal X509ChainImplOpenSSL (MonoOpenSSLX509StoreCtx storeCtx)
		{
			this.storeCtx = storeCtx.Copy ();
			this.chain = storeCtx.GetChain ();

			policy = new X509ChainPolicy ();

			untrustedChain = storeCtx.GetUntrusted ();

			if (untrustedChain != null) {
				untrusted = new X509Certificate2Collection ();
				policy.ExtraStore = untrusted;
				for (int i = 0; i < untrustedChain.Count; i++) {
					var cert = untrustedChain.GetCertificate (i);
					using (var impl = new X509CertificateImplOpenSSL (cert))
						untrusted.Add (new X509Certificate2 (impl));
				}
			}
		}

		internal X509ChainImplOpenSSL ()
		{
			chain = new MonoOpenSSLX509Chain ();
			elements = new X509ChainElementCollection ();
			policy = new X509ChainPolicy ();
		}

		public override bool IsValid {
			get { return chain != null && chain.IsValid; }
		}

		public override IntPtr Handle {
			get { return chain.Handle.DangerousGetHandle (); }
		}

		internal MonoOpenSSLX509Chain Chain {
			get {
				ThrowIfContextInvalid ();
				return chain;
			}
		}

		internal MonoOpenSSLX509StoreCtx StoreCtx {
			get {
				ThrowIfContextInvalid ();
				return storeCtx;
			}
		}

		public override X509ChainElementCollection ChainElements {
			get {
				ThrowIfContextInvalid ();
				if (elements != null)
					return elements;

				elements = new X509ChainElementCollection ();
				certificates = new X509Certificate2 [chain.Count];

				for (int i = 0; i < certificates.Length; i++) {
					var cert = chain.GetCertificate (i);
					using (var impl = new X509CertificateImplOpenSSL (cert))
						certificates [i] = new X509Certificate2 (impl);
					elements.Add (certificates [i]);
				}

				return elements;
			}
		}

		public override X509ChainPolicy ChainPolicy {
			get { return policy; }
			set { policy = value; }
		}

		public override X509ChainStatus[] ChainStatus {
			get { 
				return chainStatusList.ToArray();
			}
		}

		public override void AddStatus (X509ChainStatusFlags errorCode)
		{
			if (chainStatusList == null)
				chainStatusList = new List<X509ChainStatus>();
			chainStatusList.Add (new X509ChainStatus(errorCode));
 		}
 
		public override bool Build (X509Certificate2 certificate)
		{
			return false;
		}

		public override void Reset ()
		{
			if (certificates != null) {
				foreach (var certificate in certificates)
					certificate.Dispose ();
				certificates = null;
			}
			if (elements != null) {
				elements.Clear ();
				elements = null;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (chain != null) {
					chain.Dispose ();
					chain = null;
				}
				if (storeCtx != null) {
					storeCtx.Dispose ();
					storeCtx = null;
				}
				if (untrustedChain != null) {
					untrustedChain.Dispose ();
					untrustedChain = null;
				}
				if (untrusted != null) {
					foreach (var cert in untrusted)
						cert.Dispose ();
					untrusted = null;
				}
				if (certificates != null) {
					foreach (var cert in certificates)
						cert.Dispose ();
					certificates = null;
				}
			}
			base.Dispose (disposing);
		}
	}
}
#endif
