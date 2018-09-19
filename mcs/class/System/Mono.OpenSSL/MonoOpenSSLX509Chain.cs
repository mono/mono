//
// MonoOpenSSLX509Chain.cs
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
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono.OpenSSL
{
	class MonoOpenSSLX509Chain : MonoOpenSSLObject
	{
		internal class OpenSSLX509ChainHandle : MonoOpenSSLHandle
		{
			public OpenSSLX509ChainHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_openssl_x509_chain_free (handle);
				return true;
			}
		}

		new internal OpenSSLX509ChainHandle Handle {
			get { return (OpenSSLX509ChainHandle)base.Handle; }
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_chain_new ();

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_chain_get_count (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_chain_get_cert (IntPtr Handle, int index);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_chain_add_cert (IntPtr chain, IntPtr x509);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_chain_up_ref (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_x509_chain_free (IntPtr handle);

		public MonoOpenSSLX509Chain ()
			: base (new OpenSSLX509ChainHandle (mono_openssl_x509_chain_new ()))
		{
		}

		internal MonoOpenSSLX509Chain (OpenSSLX509ChainHandle handle)
			: base (handle)
		{
		}

		public int Count {
			get { return mono_openssl_x509_chain_get_count (Handle.DangerousGetHandle ()); }
		}

		public MonoOpenSSLX509 GetCertificate (int index)
		{
			if (index >= Count)
				throw new IndexOutOfRangeException ();
			var handle = mono_openssl_x509_chain_get_cert (
				Handle.DangerousGetHandle (), index);
			CheckError (handle != IntPtr.Zero);
			return new MonoOpenSSLX509 (new MonoOpenSSLX509.OpenSSLX509Handle (handle));
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

		public void AddCertificate (MonoOpenSSLX509 x509)
		{
			mono_openssl_x509_chain_add_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
		}

		internal MonoOpenSSLX509Chain Copy ()
		{
			var copy = mono_openssl_x509_chain_up_ref (Handle.DangerousGetHandle ());
			CheckError (copy != IntPtr.Zero);
			return new MonoOpenSSLX509Chain (new OpenSSLX509ChainHandle (copy));
		}
	}
}
#endif
