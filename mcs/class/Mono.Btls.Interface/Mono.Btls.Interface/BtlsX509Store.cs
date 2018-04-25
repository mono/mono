//
// BtlsX509Store.cs
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
using System;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Btls.Interface
{
	public class BtlsX509Store : BtlsObject
	{
		new internal MonoBtlsX509Store Instance {
			get { return (MonoBtlsX509Store)base.Instance; }
		}

		internal BtlsX509Store (MonoBtlsX509Store store)
			: base (store)
		{
		}

		public void LoadLocations (string file, string path)
		{
			Instance.LoadLocations (file, path);
		}

		public void AddTrustedRoots ()
		{
			Instance.AddTrustedRoots ();
		}

		public void AddCertificate (BtlsX509 x509)
		{
			Instance.AddCertificate (x509.Instance);
		}

		public int GetCount ()
		{
			return Instance.GetCount ();
		}

		public void AddLookup (X509CertificateCollection certificates, BtlsX509TrustKind trust)
		{
			Instance.AddCollection (certificates, (MonoBtlsX509TrustKind)trust);
		}

		static MonoBtlsX509FileType GetFileType (BtlsX509Format format)
		{
			switch (format) {
			case BtlsX509Format.DER:
				return MonoBtlsX509FileType.ASN1;
			case BtlsX509Format.PEM:
				return MonoBtlsX509FileType.PEM;
			default:
				throw new NotSupportedException ();
			}
		}

		public void AddDirectoryLookup (string dir, BtlsX509Format format)
		{
			Instance.AddDirectoryLookup (dir, GetFileType (format));
		}

		public void AddFileLookup (string file, BtlsX509Format format)
		{
			Instance.AddFileLookup (file, GetFileType (format));
		}

	}
}

