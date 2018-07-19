//
// X509PalImpl.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin, Inc.
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
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;

namespace Mono
{
	abstract class X509PalImpl
	{
		public abstract X509CertificateImpl Import (byte[] data);

		public abstract X509Certificate2Impl Import (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags);

		public abstract X509Certificate2Impl Import (X509Certificate cert);

		static byte[] PEM (string type, byte[] data)
		{
			string pem = Encoding.ASCII.GetString (data);
			string header = String.Format ("-----BEGIN {0}-----", type);
			string footer = String.Format ("-----END {0}-----", type);
			int start = pem.IndexOf (header) + header.Length;
			int end = pem.IndexOf (footer, start);
			string base64 = pem.Substring (start, (end - start));
			return Convert.FromBase64String (base64);
		}

		protected static byte[] ConvertData (byte[] data)
		{
			if (data == null || data.Length == 0)
				return data;

			// does it looks like PEM ?
			if (data[0] != 0x30) {
				try {
					return PEM ("CERTIFICATE", data);
				} catch {
					// let the implementation take care of it.
				}
			}
			return data;
		}

		internal X509Certificate2Impl ImportFallback (byte[] data)
		{
			data = ConvertData (data);

			var impl = new X509Certificate2ImplMono ();
			using (var handle = new SafePasswordHandle ((string)null))
				impl.Import (data, handle, X509KeyStorageFlags.DefaultKeySet);
			return impl;
		}

		internal X509Certificate2Impl ImportFallback (byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags)
		{
			var impl = new X509Certificate2ImplMono ();
			impl.Import (data, password, keyStorageFlags);
			return impl;
		}
	}
}
