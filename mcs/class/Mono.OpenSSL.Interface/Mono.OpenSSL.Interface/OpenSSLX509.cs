//
// OpenSSLX509.cs
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
using System.IO;
using System.Security.Cryptography;

namespace Mono.OpenSSL.Interface
{
	public class OpenSSLX509 : OpenSSLObject
	{
		new internal MonoOpenSSLX509 Instance {
			get { return (MonoOpenSSLX509)base.Instance; }
		}

		internal OpenSSLX509 (MonoOpenSSLX509 x509)
			: base (x509)
		{
		}

		public OpenSSLX509Name GetSubjectName ()
		{
			return new OpenSSLX509Name (Instance.GetSubjectName ());
		}

		public OpenSSLX509Name GetIssuerName ()
		{
			return new OpenSSLX509Name (Instance.GetIssuerName ());
		}

		public string GetSubjectNameString ()
		{
			return Instance.GetSubjectNameString ();
		}

		public string GetIssuerNameString ()
		{
			return Instance.GetIssuerNameString ();
		}

		public byte[] GetRawData (OpenSSLX509Format format)
		{
			return Instance.GetRawData ((MonoOpenSSLX509Format)format);
		}

		public byte[] GetCertHash ()
		{
			return Instance.GetCertHash ();
		}

		public DateTime GetNotBefore ()
		{
			return Instance.GetNotBefore ();
		}

		public DateTime GetNotAfter ()
		{
			return Instance.GetNotAfter ();
		}

		public byte[] GetPublicKeyData ()
		{
			return Instance.GetPublicKeyData ();
		}

		public byte[] GetSerialNumber (bool mono_style)
		{
			return Instance.GetSerialNumber (mono_style);
		}

		public int GetVersion ()
		{
			return Instance.GetVersion ();
		}

		public Oid GetSignatureAlgorithm ()
		{
			return Instance.GetSignatureAlgorithm ();
		}

		public AsnEncodedData GetPublicKeyAsn1 ()
		{
			return Instance.GetPublicKeyAsn1 ();
		}

		public AsnEncodedData GetPublicKeyParameters ()
		{
			return Instance.GetPublicKeyParameters (); 
		}

		public long GetSubjectNameHash ()
		{
			using (var name = GetSubjectName ())
				return name.GetHash ();
		}

		public void Print (Stream stream)
		{
			using (var bio = MonoOpenSSLBio.CreateMonoStream (stream))
				Instance.Print (bio);
		}

		public void ExportAsPEM (Stream stream, bool includeHumanReadableForm)
		{
			using (var bio = MonoOpenSSLBio.CreateMonoStream (stream))
				Instance.ExportAsPEM (bio, includeHumanReadableForm);
		}
	}
}

