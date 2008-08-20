//
// X509RawDataKeyIdentifierClause.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Policy;

namespace System.IdentityModel.Tokens
{
	public class X509RawDataKeyIdentifierClause : BinaryKeyIdentifierClause
	{
		public X509RawDataKeyIdentifierClause (byte [] certificateRawData)
			: base (null, certificateRawData, true)
		{
		}

		public X509RawDataKeyIdentifierClause (X509Certificate2 certificate)
			: base (null, certificate.RawData, true)
		{
			this.cert = certificate;
		}

		X509Certificate2 cert;

		public override bool CanCreateKey {
			get { return true; }
		}

		public override SecurityKey CreateKey ()
		{
			if (cert == null)
				cert = new X509Certificate2 (GetX509RawData ());
			return new X509AsymmetricSecurityKey (cert);
		}

		[MonoTODO ("Not sure what should be returned when there are public/private pair key and public-only key")]
		public byte [] GetX509RawData ()
		{
			return GetRawBuffer ();
		}

		[MonoTODO ("Not sure what should be returned when there are public/private pair key and public-only key")]
		public bool Matches (X509Certificate2 certificate)
		{
			return Matches (certificate.RawData);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
