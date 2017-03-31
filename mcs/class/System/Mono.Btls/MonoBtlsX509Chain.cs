//
// MonoBtlsX509Chain.cs
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
#if SECURITY_DEP && MONO_FEATURE_BTLS
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono.Btls
{
	class MonoBtlsX509Chain : MonoBtlsObject
	{
		internal class BoringX509ChainHandle : MonoBtlsHandle
		{
			public BoringX509ChainHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_x509_chain_free (handle);
				return true;
			}
		}

		new internal BoringX509ChainHandle Handle {
			get { return (BoringX509ChainHandle)base.Handle; }
		}

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_chain_new ();

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_chain_get_count (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_chain_get_cert (IntPtr Handle, int index);

		[DllImport (BTLS_DYLIB)]
		extern static int mono_btls_x509_chain_add_cert (IntPtr chain, IntPtr x509);

		[DllImport (BTLS_DYLIB)]
		extern static IntPtr mono_btls_x509_chain_up_ref (IntPtr handle);

		[DllImport (BTLS_DYLIB)]
		extern static void mono_btls_x509_chain_free (IntPtr handle);

		public MonoBtlsX509Chain ()
			: base (new BoringX509ChainHandle (mono_btls_x509_chain_new ()))
		{
		}

		internal MonoBtlsX509Chain (BoringX509ChainHandle handle)
			: base (handle)
		{
		}

		public int Count {
			get { return mono_btls_x509_chain_get_count (Handle.DangerousGetHandle ()); }
		}

		public MonoBtlsX509 GetCertificate (int index)
		{
			if (index >= Count)
				throw new IndexOutOfRangeException ();
			var handle = mono_btls_x509_chain_get_cert (
				Handle.DangerousGetHandle (), index);
			CheckError (handle != IntPtr.Zero);
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (handle));
		}

		public void Dump ()
		{
			Console.Error.WriteLine ("CHAIN: {0:x} {1}", Handle, Count);
			for (int i = 0; i < Count; i++) {
				using (var cert = GetCertificate (i)) {
					Console.Error.WriteLine ("  CERT #{0}: {1}", i, cert.GetSubjectNameString ());
				}
			}
		}

		public void AddCertificate (MonoBtlsX509 x509)
		{
			mono_btls_x509_chain_add_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
		}

		internal MonoBtlsX509Chain Copy ()
		{
			var copy = mono_btls_x509_chain_up_ref (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoBtlsX509Chain (new BoringX509ChainHandle (copy));
		}
	}
}
#endif
