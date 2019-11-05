//
// MonoBtlsX509LookupWinCrypto.cs
//
// Author:
//       Vincent Povirk <vincent@codeweavers.com>
//
// Copyright (c) 2019 Vincent Povirk for CodeWeavers
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
#if SECURITY_DEP && MONO_FEATURE_BTLS && MX_WINCRYPTO
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Btls
{
	internal class MonoBtlsX509LookupWinCrypto : MonoBtlsX509LookupMono
	{
		public StoreLocation Location { get; set; }

		protected override MonoBtlsX509 OnGetBySubject (MonoBtlsX509Name name)
		{
			byte[] raw_data = name.GetRawData (false);
			var x509_name = new X500DistinguishedName (raw_data);
			using (var certstore = new X509Store (StoreName.Root, Location))
			{
				try
				{
					certstore.Open (OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
				}
				catch (CryptographicException)
				{
					return null;
				}
				var matches = certstore.Certificates.Find (X509FindType.FindBySubjectDistinguishedName, x509_name.Name, false);
				if (matches.Count >= 1)
				{
					// FIXME: Which one to use if more than 1 match?
					return MonoBtlsX509.LoadFromData (matches[0].RawData, MonoBtlsX509Format.DER);
				}
				return null;
			}
		}
	}
}
#endif

