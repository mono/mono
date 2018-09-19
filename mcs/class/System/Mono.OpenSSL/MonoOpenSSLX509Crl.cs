//
// MonoOpenSSLX509Crl.cs
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
	class MonoOpenSSLX509Crl : MonoOpenSSLObject
	{
		internal class OpenSSLX509CrlHandle : MonoOpenSSLHandle
		{
			public OpenSSLX509CrlHandle (IntPtr handle)
				: base (handle, true)
			{
			}

			protected override bool ReleaseHandle ()
			{
				if (handle != IntPtr.Zero)
					mono_openssl_x509_crl_free (handle);
				return true;
			}

			public IntPtr StealHandle ()
			{
				var retval = Interlocked.Exchange (ref handle, IntPtr.Zero);
				return retval;
			}
		}

		new internal OpenSSLX509CrlHandle Handle {
			get { return (OpenSSLX509CrlHandle)base.Handle; }
		}

		internal MonoOpenSSLX509Crl (OpenSSLX509CrlHandle handle) 
			: base (handle)
		{
		}

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_crl_ref (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_crl_from_data (IntPtr data, int len, MonoOpenSSLX509Format format);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_crl_get_by_cert (IntPtr handle, IntPtr x509);

		[DllImport (OPENSSL_DYLIB)]
		unsafe extern static IntPtr mono_openssl_x509_crl_get_by_serial (IntPtr handle, void *serial, int len);

		[DllImport (OPENSSL_DYLIB)]
		extern static int mono_openssl_x509_crl_get_revoked_count (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_crl_get_revoked (IntPtr handle, int index);

		[DllImport (OPENSSL_DYLIB)]
		extern static long mono_openssl_x509_crl_get_last_update (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static long mono_openssl_x509_crl_get_next_update (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static long mono_openssl_x509_crl_get_version (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static IntPtr mono_openssl_x509_crl_get_issuer (IntPtr handle);

		[DllImport (OPENSSL_DYLIB)]
		extern static void mono_openssl_x509_crl_free (IntPtr handle);

		public static MonoOpenSSLX509Crl LoadFromData (byte[] buffer, MonoOpenSSLX509Format format)
		{
			var data = Marshal.AllocHGlobal (buffer.Length);
			if (data == IntPtr.Zero)
				throw new OutOfMemoryException ();

			try {
				Marshal.Copy (buffer, 0, data, buffer.Length);
				var crl = mono_openssl_x509_crl_from_data (data, buffer.Length, format);
				if (crl == IntPtr.Zero)
					throw new MonoOpenSSLException ("Failed to read CRL from data.");

				return new MonoOpenSSLX509Crl (new OpenSSLX509CrlHandle (crl));
			} finally {
				Marshal.FreeHGlobal (data);
			}
		}

		public MonoOpenSSLX509Revoked GetByCert (MonoOpenSSLX509 x509)
		{
			var revoked = mono_openssl_x509_crl_get_by_cert (
				Handle.DangerousGetHandle (),
				x509.Handle.DangerousGetHandle ());
			if (revoked == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509Revoked (new MonoOpenSSLX509Revoked.OpenSSLX509RevokedHandle (revoked));
		}

		public unsafe MonoOpenSSLX509Revoked GetBySerial (byte[] serial)
		{
			fixed (void *ptr = serial)
			{
				var revoked = mono_openssl_x509_crl_get_by_serial (
					Handle.DangerousGetHandle (), ptr, serial.Length);
				if (revoked == IntPtr.Zero)
					return null;
				return new MonoOpenSSLX509Revoked (new MonoOpenSSLX509Revoked.OpenSSLX509RevokedHandle (revoked));
			}
		}

		public int GetRevokedCount ()
		{
			return mono_openssl_x509_crl_get_revoked_count (Handle.DangerousGetHandle ());
		}

		public MonoOpenSSLX509Revoked GetRevoked (int index)
		{
			if (index >= GetRevokedCount ())
				throw new ArgumentOutOfRangeException ();

			var revoked = mono_openssl_x509_crl_get_revoked (
				Handle.DangerousGetHandle (), index);
			if (revoked == IntPtr.Zero)
				return null;
			return new MonoOpenSSLX509Revoked (new MonoOpenSSLX509Revoked.OpenSSLX509RevokedHandle (revoked));
		}

		public DateTime GetLastUpdate ()
		{
			var ticks = mono_openssl_x509_crl_get_last_update (Handle.DangerousGetHandle ());
			return new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds (ticks);
		}

		public DateTime GetNextUpdate ()
		{
			var ticks = mono_openssl_x509_crl_get_next_update (Handle.DangerousGetHandle ());
			return new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds (ticks);
		}

		public long GetVersion ()
		{
			return mono_openssl_x509_crl_get_version (Handle.DangerousGetHandle ());
		}

		public MonoOpenSSLX509Name GetIssuerName ()
		{
			var handle = mono_openssl_x509_crl_get_issuer (Handle.DangerousGetHandle ());
			CheckError (handle != IntPtr.Zero);
			return new MonoOpenSSLX509Name (new MonoOpenSSLX509Name.OpenSSLX509NameHandle (handle, false));
		}
	}
}
#endif
