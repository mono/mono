//
// MonoOpenSSLX509Revoked.cs
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
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace Mono.OpenSSL
{
	class MonoOpenSSLX509Revoked : MonoOpenSSLObject
	{
		internal class OpenSSLX509RevokedHandle : MonoOpenSSLHandle
		{
			public OpenSSLX509RevokedHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				if (handle != IntPtr.Zero)
					mono_openssl_x509_revoked_free (handle);
				return true;
			}

			public IntPtr StealHandle ()
			{
				var retval = Interlocked.Exchange (ref handle, IntPtr.Zero);
				return retval;
			}
		}

		new internal OpenSSLX509RevokedHandle Handle {
			get { return (OpenSSLX509RevokedHandle)base.Handle; }
		}

		internal MonoOpenSSLX509Revoked (OpenSSLX509RevokedHandle handle)
			: base (handle)
		{
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_revoked_get_serial_number (IntPtr handle, IntPtr data, int size);

		[DllImport (OPENSSL_DYLIB)]
		extern static long mono_openssl_x509_revoked_get_revocation_date (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_revoked_get_reason (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_revoked_get_sequence (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_x509_revoked_free (IntPtr handle);

		public byte[] GetSerialNumber ()
		{
			int size = 256;
			IntPtr data = Marshal.AllocHGlobal (size);
			try {
				var ret = mono_openssl_x509_revoked_get_serial_number (
					Handle.DangerousGetHandle (), data, size);
				CheckError (ret > 0);
				var buffer = new byte[ret];
				Marshal.Copy (data, buffer, 0, ret);
				return buffer;
			} finally {
				if (data != IntPtr.Zero)
					Marshal.FreeHGlobal (data);
			}
		}

		public DateTime GetRevocationDate ()
		{
			var ticks = mono_openssl_x509_revoked_get_revocation_date (
				Handle.DangerousGetHandle ());
			return new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds (ticks);
		}

		public int GetReason ()
		{
			return mono_openssl_x509_revoked_get_reason (Handle.DangerousGetHandle ());
		}

		public int GetSequence ()
		{
			return mono_openssl_x509_revoked_get_sequence (Handle.DangerousGetHandle ());
		}
	}
}
#endif
