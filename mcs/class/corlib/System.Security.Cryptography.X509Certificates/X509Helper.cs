//
// X509Helpers.cs: X.509 helper and utility functions.
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2015 Xamarin, Inc. (http://www.xamarin.com)
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
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
#if !MOBILE
using System.Security.Permissions;
#endif
using Mono;

namespace System.Security.Cryptography.X509Certificates
{
	static partial class X509Helper
	{
		static ISystemCertificateProvider CertificateProvider => DependencyInjector.SystemProvider.CertificateProvider;

		public static X509CertificateImpl InitFromCertificate (X509Certificate cert)
		{
			return CertificateProvider.Import (cert, CertificateImportFlags.None);
		}

		public static X509CertificateImpl InitFromCertificate (X509CertificateImpl impl)
		{
			return impl?.Clone ();
		}

		public static bool IsValid (X509CertificateImpl impl)
		{
			return impl != null && impl.IsValid;
		}

		internal static void ThrowIfContextInvalid (X509CertificateImpl impl)
		{
			if (!IsValid (impl))
				throw GetInvalidContextException ();
		}

		internal static Exception GetInvalidContextException ()
		{
			return new CryptographicException (Locale.GetText ("Certificate instance is empty."));
		}

		public static X509CertificateImpl Import (byte[] rawData)
		{
			return CertificateProvider.Import (rawData);
		}

		public static X509CertificateImpl Import (byte[] rawData, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			return CertificateProvider.Import (rawData, password, keyStorageFlags);
		}

		public static byte[] Export (X509CertificateImpl impl, X509ContentType contentType, SafePasswordHandle password)
		{
			ThrowIfContextInvalid (impl);
			return impl.Export (contentType, password);
		}

		public static bool Equals (X509CertificateImpl first, X509CertificateImpl second)
		{
			if (!IsValid (first) || !IsValid (second))
				return false;

			bool result;
			if (first.Equals (second, out result))
				return result;

			var firstRaw = first.RawData;
			var secondRaw = second.RawData;

			if (firstRaw == null)
				return secondRaw == null;
			else if (secondRaw == null)
				return false;

			if (firstRaw.Length != secondRaw.Length)
				return false;

			for (int i = 0; i < firstRaw.Length; i++) {
				if (firstRaw [i] != secondRaw [i])
					return false;
			}

			return true;
		}

		// almost every byte[] returning function has a string equivalent
		// sadly the BitConverter insert dash between bytes :-(
		public static string ToHexString (byte[] data)
		{
			if (data != null) {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < data.Length; i++)
					sb.Append (data[i].ToString ("X2"));
				return sb.ToString ();
			}
			else
				return null;
		}
	}
}
