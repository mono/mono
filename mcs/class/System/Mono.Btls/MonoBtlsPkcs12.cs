﻿//
// MonoBtlsPkcs12.cs
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
#if SECURITY_DEP
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono.Btls
{
	class MonoBtlsPkcs12 : MonoBtlsObject
	{
		internal class BoringPkcs12Handle : MonoBtlsHandle
		{
			public BoringPkcs12Handle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				mono_btls_pkcs12_free (handle);
				return true;
			}
		}

		new internal BoringPkcs12Handle Handle {
			get { return (BoringPkcs12Handle)base.Handle; }
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static void mono_btls_pkcs12_free (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_pkcs12_new ();

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_pkcs12_get_count (IntPtr handle);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_pkcs12_get_cert (IntPtr Handle, int index);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_pkcs12_add_cert (IntPtr chain, IntPtr x509);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern unsafe static int mono_btls_pkcs12_import (IntPtr chain, void* data, int len, IntPtr password);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static int mono_btls_pkcs12_has_private_key (IntPtr pkcs12);

		[MethodImpl (MethodImplOptions.InternalCall)]
		extern static IntPtr mono_btls_pkcs12_get_private_key (IntPtr pkcs12);

		internal MonoBtlsPkcs12 ()
			: base (new BoringPkcs12Handle (mono_btls_pkcs12_new ()))
		{
		}

		internal MonoBtlsPkcs12 (BoringPkcs12Handle handle)
			: base (handle)
		{
		}

		MonoBtlsKey privateKey;

		public int Count {
			get { return mono_btls_pkcs12_get_count (Handle.DangerousGetHandle ()); }
		}

		public MonoBtlsX509 GetCertificate (int index)
		{
			if (index >= Count)
				throw new IndexOutOfRangeException ();
			var handle = mono_btls_pkcs12_get_cert (Handle.DangerousGetHandle (), index);
			CheckError (handle != IntPtr.Zero);
			return new MonoBtlsX509 (new MonoBtlsX509.BoringX509Handle (handle));
		}

		public void AddCertificate (MonoBtlsX509 x509)
		{
			mono_btls_pkcs12_add_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
		}

		public unsafe void Import (byte[] buffer, string password)
		{
			var passptr = IntPtr.Zero;
			fixed (void* ptr = buffer)
			try {
				if (password != null)
					passptr = Marshal.StringToHGlobalAnsi (password);
				var ret = mono_btls_pkcs12_import (
					Handle.DangerousGetHandle (), ptr,
					buffer.Length, passptr);
				CheckError (ret);
			} finally {
				if (passptr != IntPtr.Zero)
					Marshal.FreeHGlobal (passptr);
			}
		}

		public bool HasPrivateKey {
			get { return mono_btls_pkcs12_has_private_key (Handle.DangerousGetHandle ()) != 0; }
		}

		public MonoBtlsKey GetPrivateKey ()
		{
			if (!HasPrivateKey)
				throw new InvalidOperationException ();
			if (privateKey == null) {
				var handle = mono_btls_pkcs12_get_private_key (Handle.DangerousGetHandle ());
				CheckError (handle != IntPtr.Zero);
				privateKey = new MonoBtlsKey (new MonoBtlsKey.BoringKeyHandle (handle));
			}
			return privateKey;
		}
	}
}
#endif
