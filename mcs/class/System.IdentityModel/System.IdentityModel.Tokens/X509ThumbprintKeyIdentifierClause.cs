//
// X509ThumbprintKeyIdentifierClause.cs
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
using System.IdentityModel.Claims;

namespace System.IdentityModel.Tokens
{
	public class X509ThumbprintKeyIdentifierClause : BinaryKeyIdentifierClause
	{
		public X509ThumbprintKeyIdentifierClause (byte [] thumbprint)
			: base (null, thumbprint, true)
		{
		}

		public X509ThumbprintKeyIdentifierClause (X509Certificate2 certificate)
			: base (null, certificate.GetCertHash (), true)
		{
		}

		public byte [] GetX509Thumbprint ()
		{
			return GetBuffer ();
		}

		public bool Matches (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			byte [] b1 = GetRawBuffer ();
			byte [] b2 = certificate.GetCertHash ();
			if (b1.Length != b2.Length)
				return false;
			for (int i = 0; i < b1.Length; i++)
				if (b1 [i] != b2 [i])
					return false;
			return true;
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
