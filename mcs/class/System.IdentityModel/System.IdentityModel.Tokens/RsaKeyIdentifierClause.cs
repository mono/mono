//
// RsaKeyIdentifierClause.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005-2006 Novell, Inc.  http://www.novell.com
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
using System.Xml;
using System.IdentityModel.Policy;
using System.Security.Cryptography;

namespace System.IdentityModel.Tokens
{
	public class RsaKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		public RsaKeyIdentifierClause (RSA rsa)
			: base (null)
		{
			this.rsa = rsa;
		}

		RSA rsa;

		public RSA Rsa {
			get { return rsa; }
		}

		public override bool CanCreateKey {
			get { return true; }
		}

		public override SecurityKey CreateKey ()
		{
			return new RsaSecurityKey (rsa);
		}

		public byte [] GetExponent ()
		{
			return rsa.ExportParameters (false).Exponent;
		}

		[MonoTODO]
		public void WriteExponentAsBase64 (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		public byte [] GetModulus ()
		{
			return rsa.ExportParameters (false).Modulus;
		}

		[MonoTODO]
		public void WriteModulusAsBase64 (XmlWriter writer)
		{
			throw new NotImplementedException ();
		}

		public override bool Matches (SecurityKeyIdentifierClause keyIdentifierClause)
		{
			if (keyIdentifierClause == null)
				throw new ArgumentNullException ("keyIdentifierClause");
			RsaKeyIdentifierClause rkic =
				keyIdentifierClause as RsaKeyIdentifierClause;
			return rkic != null && Matches (rkic.Rsa);
		}

		public bool Matches (RSA rsa)
		{
			// hmm, there should be more decent way to compare ...
			return rsa.ToXmlString (false) == this.rsa.ToXmlString (false);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
