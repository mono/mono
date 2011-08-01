//
// X509IssuerSerialKeyIdentifierClause.cs
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
using System.Security.Cryptography.X509Certificates;
#if !TARGET_DOTNET
using Mono.Math;
#endif

namespace System.IdentityModel.Tokens
{
	public class X509IssuerSerialKeyIdentifierClause : SecurityKeyIdentifierClause
	{
		static byte [] FromBinHex (string s)
		{
			byte [] bytes = new byte [s.Length / 2];
			for (int i = 0; i < bytes.Length; i++)
				bytes [i] = (byte) (DecodeHex (s [i * 2]) * 16 + DecodeHex (s [i * 2 + 1]));
			return bytes;
		}

		static byte DecodeHex (char c)
		{
			return  (byte) (c <= '9' ? c - '0' : c <= 'F' ? c - 'A' + 10 : c - 'a' + 10);
		}

		static string ToDecimalString (string hexString)
		{
#if TARGET_DOTNET
			throw new NotImplementedException ();
#else			
           // http://tools.ietf.org/html/rfc5280#section-4.1.2.2
           // We SHOULD support negative numbers
           var bytes = FromBinHex (hexString);
			
            var negative = bytes.Length > 0 && bytes[0] >= 0x80;
			if (negative) 
				for (int i = 0; i < bytes.Length; i++) 
					bytes[i] = (byte) ~ bytes[i];
        	
			var big = new BigInteger (bytes);
			if (negative) { 
				big = big + 1;
				return "-" + big.ToString();
			} else 
				return big.ToString ();
#endif
        }

		public X509IssuerSerialKeyIdentifierClause (X509Certificate2 certificate)
			: base (null)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			name = certificate.IssuerName.Name;
			serial = ToDecimalString (certificate.SerialNumber);
		}

		public X509IssuerSerialKeyIdentifierClause (string issuerName, string issuerSerialNumber)
			: base (null)
		{
			name = issuerName;
			serial = issuerSerialNumber;
		}

		string name, serial;

		public string IssuerName {
			get { return name; }
		}

		public string IssuerSerialNumber {
			get { return serial; }
		}

		public override bool Matches (SecurityKeyIdentifierClause clause)
		{
			X509IssuerSerialKeyIdentifierClause other =
				clause as X509IssuerSerialKeyIdentifierClause;
			return other != null && Matches (other.name, other.serial);
		}

		public bool Matches (X509Certificate2 certificate)
		{
			return name == certificate.IssuerName.Name &&
			       serial == ToDecimalString (certificate.SerialNumber);
		}

		public bool Matches (string issuerName, string issuerSerialNumber)
		{
			return name == issuerName && serial == issuerSerialNumber;
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}
	}
}
