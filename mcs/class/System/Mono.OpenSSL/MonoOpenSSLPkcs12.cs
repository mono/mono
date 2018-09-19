﻿//
// MonoOpenSSLPkcs12.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
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
using Microsoft.Win32.SafeHandles;

namespace Mono.OpenSSL
{
	class MonoOpenSSLPkcs12 : MonoOpenSSLObject
	{
		internal class OpenSSLPkcs12Handle : MonoOpenSSLHandle
		{
			public OpenSSLPkcs12Handle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_openssl_pkcs12_free (handle);
				return true;
			}
		}

		new internal OpenSSLPkcs12Handle Handle {
			get { return (OpenSSLPkcs12Handle)base.Handle; }
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_pkcs12_free (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_pkcs12_new ();

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_pkcs12_get_count (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_pkcs12_get_cert (IntPtr Handle, int index);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_pkcs12_add_cert (IntPtr chain, IntPtr x509);

		[DllImport (OPENSSL_DYLIB)]
		extern unsafe static int mono_openssl_pkcs12_import (IntPtr chain, void* data, int len, SafePasswordHandle password);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_pkcs12_has_private_key (IntPtr pkcs12);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_pkcs12_get_private_key (IntPtr pkcs12);

		internal MonoOpenSSLPkcs12 ()
			: base (new OpenSSLPkcs12Handle (mono_openssl_pkcs12_new ()))
		{
		}

		internal MonoOpenSSLPkcs12 (OpenSSLPkcs12Handle handle)
			: base (handle)
		{
		}

		MonoOpenSSLKey privateKey;

		public int Count {
			get { return mono_openssl_pkcs12_get_count (Handle.DangerousGetHandle ()); }
		}

		public MonoOpenSSLX509 GetCertificate (int index)
		{
			if (index >= Count)
				throw new IndexOutOfRangeException ();
			var handle = mono_openssl_pkcs12_get_cert (Handle.DangerousGetHandle (), index);
			CheckError (handle != IntPtr.Zero);
			return new MonoOpenSSLX509 (new MonoOpenSSLX509.OpenSSLX509Handle (handle));
		}

		public void AddCertificate (MonoOpenSSLX509 x509)
		{
			mono_openssl_pkcs12_add_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
		}

		public unsafe void Import (byte[] buffer, SafePasswordHandle password)
		{
			fixed (void* ptr = buffer) {
 				var ret = mono_openssl_pkcs12_import (Handle.DangerousGetHandle (), 
								      ptr, buffer.Length, password);
 				CheckError (ret);
			}
		}

		public bool HasPrivateKey {
			get { return mono_openssl_pkcs12_has_private_key (Handle.DangerousGetHandle ()) != 0; }
		}

		public MonoOpenSSLKey GetPrivateKey ()
		{
			if (!HasPrivateKey)
				throw new InvalidOperationException ();
			if (privateKey == null) {
				var handle = mono_openssl_pkcs12_get_private_key (Handle.DangerousGetHandle ());
				CheckError (handle != IntPtr.Zero);
				privateKey = new MonoOpenSSLKey (new MonoOpenSSLKey.OpenSSLKeyHandle (handle));
			}
			return privateKey;
		}
	}
}
#endif
