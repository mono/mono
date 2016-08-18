//
// System.Security.Cryptography.X509Certificates.X509Chain
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc. (http://www.xamarin.com)
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

#if SECURITY_DEP

#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
using MX = MonoSecurity::Mono.Security.X509;
#else
using MX = Mono.Security.X509;
#endif

using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Text;

namespace System.Security.Cryptography.X509Certificates {

	public class X509Chain : IDisposable {

		X509ChainImpl impl;

		static X509ChainStatus[] Empty = new X509ChainStatus [0];

		internal X509ChainImpl Impl {
			get {
				X509Helper2.ThrowIfContextInvalid (impl);
				return impl;
			}
		}

		internal bool IsValid {
			get { return X509Helper2.IsValid (impl); }
		}

		internal void ThrowIfContextInvalid ()
		{
			X509Helper2.ThrowIfContextInvalid (impl);
		}

		// constructors

		public X509Chain ()
			: this (false)
		{
		}

		public X509Chain (bool useMachineContext) 
		{
			impl = X509Helper2.CreateChainImpl (useMachineContext);
		}

		internal X509Chain (X509ChainImpl impl)
		{
			X509Helper2.ThrowIfContextInvalid (impl);
			this.impl = impl;
		}

		[MonoTODO ("Mono's X509Chain is fully managed. All handles are invalid.")]
		public X509Chain (IntPtr chainContext)
		{
			// CryptoAPI compatibility (unmanaged handle)
			throw new NotSupportedException ();
		}

		// properties

		[MonoTODO ("Mono's X509Chain is fully managed. Always returns IntPtr.Zero.")]
		public IntPtr ChainContext {
			get {
				if (impl != null && impl.IsValid)
					return impl.Handle;
				return IntPtr.Zero;
			}
		}

		public X509ChainElementCollection ChainElements {
			get { return Impl.ChainElements; }
		}

		public X509ChainPolicy ChainPolicy {
			get { return Impl.ChainPolicy; }
			set { Impl.ChainPolicy = value; }
		}

		public X509ChainStatus[] ChainStatus {
			get { return Impl.ChainStatus; }
		}

		public SafeX509ChainHandle SafeHandle {
			get { throw new NotImplementedException (); }
		}

		// methods

		[MonoTODO ("Not totally RFC3280 compliant, but neither is MS implementation...")]
		public bool Build (X509Certificate2 certificate)
		{
			return Impl.Build (certificate);
		}

		public void Reset () 
		{
			Impl.Reset ();
		}

		// static methods

		public static X509Chain Create ()
		{
#if FULL_AOT_RUNTIME
			return new X509Chain ();
#else
			return (X509Chain) CryptoConfig.CreateFromName ("X509Chain");
#endif
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (impl != null) {
				impl.Dispose ();
				impl = null;
			}
		}

		~X509Chain ()
		{
			Dispose (false);
		}
	}
}
#else
namespace System.Security.Cryptography.X509Certificates {
	public class X509Chain {
		public bool Build (X509Certificate2 cert)
		{
			return false;
		}
	}
}
#endif

