//
// System.Security.Cryptography.X509Certificates.X500DistinguishedName
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.Collections;
using System.Text;

using Mono.Security;
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	[MonoTODO ("Some X500DistinguishedNameFlags options aren't supported, like DoNotUsePlusSign, DoNotUseQuotes and ForceUTF8Encoding")]
	public sealed class X500DistinguishedName : AsnEncodedData {

		private const X500DistinguishedNameFlags AllFlags = X500DistinguishedNameFlags.Reversed |
			X500DistinguishedNameFlags.UseSemicolons | X500DistinguishedNameFlags.DoNotUsePlusSign | 
			X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseCommas | 
			X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding | 
			X500DistinguishedNameFlags.UseT61Encoding | X500DistinguishedNameFlags.ForceUTF8Encoding;

		private string name;


		public X500DistinguishedName (AsnEncodedData encodedDistinguishedName)
		{
			if (encodedDistinguishedName == null)
				throw new ArgumentNullException ("encodedDistinguishedName");

			RawData = encodedDistinguishedName.RawData;
			if (RawData.Length > 0)
				DecodeRawData ();
			else
				name = String.Empty;
		}

		public X500DistinguishedName (byte[] encodedDistinguishedName)
		{
			if (encodedDistinguishedName == null)
				throw new ArgumentNullException ("encodedDistinguishedName");

			Oid = new Oid ();
			RawData = encodedDistinguishedName;
			if (encodedDistinguishedName.Length > 0)
				DecodeRawData ();
			else
				name = String.Empty;
		}

		public X500DistinguishedName (string distinguishedName)
			: this (distinguishedName, X500DistinguishedNameFlags.Reversed)
		{
		}

		public X500DistinguishedName (string distinguishedName, X500DistinguishedNameFlags flag)
		{
			if (distinguishedName == null)
				throw new ArgumentNullException ("distinguishedName");
			if ((flag != 0) && ((flag & AllFlags) == 0))
				throw new ArgumentException ("flag");

			Oid = new Oid ();
			if (distinguishedName.Length == 0) {
				// empty (0x00) ASN.1 sequence (0x30)
				RawData = new byte [2] { 0x30, 0x00 };
				DecodeRawData ();
			} else {
				ASN1 dn = MX.X501.FromString (distinguishedName);
				if ((flag & X500DistinguishedNameFlags.Reversed) != 0) {
					ASN1 rdn = new ASN1 (0x30);
					for (int i = dn.Count - 1; i >= 0; i--)	
						rdn.Add (dn [i]);
					dn = rdn;
				}
				RawData = dn.GetBytes ();
				if (flag == X500DistinguishedNameFlags.None)
					name = distinguishedName;
				else
					name = Decode (flag);
			}
		}

		public X500DistinguishedName (X500DistinguishedName distinguishedName)
		{
			if (distinguishedName == null)
				throw new ArgumentNullException ("distinguishedName");

			Oid = new Oid ();
			RawData = distinguishedName.RawData;
			name = distinguishedName.name;
		}


		public string Name {
			get { return name; }
		}


		public string Decode (X500DistinguishedNameFlags flag)
		{
			if ((flag != 0) && ((flag & AllFlags) == 0))
				throw new ArgumentException ("flag");

			if (RawData.Length == 0)
				return String.Empty;

			// Mono.Security reversed isn't the same as fx 2.0 (which is the reverse of 1.x)
			bool reversed = ((flag & X500DistinguishedNameFlags.Reversed) != 0);
			bool quotes = ((flag & X500DistinguishedNameFlags.DoNotUseQuotes) == 0);
			string separator = GetSeparator (flag);

			ASN1 rdn = new ASN1 (RawData);
			return MX.X501.ToString (rdn, reversed, separator, quotes);
		}

		public override string Format (bool multiLine)
		{
			if (multiLine) {
				string s = Decode (X500DistinguishedNameFlags.UseNewLines);
				if (s.Length > 0)
					return s + Environment.NewLine;
				else
					return s;
			} else {
				return Decode (X500DistinguishedNameFlags.UseCommas);
			}
		}

		// private stuff

		private static string GetSeparator (X500DistinguishedNameFlags flag)
		{
			if ((flag & X500DistinguishedNameFlags.UseSemicolons) != 0)
				return "; ";
			if ((flag & X500DistinguishedNameFlags.UseCommas) != 0)
				return ", ";
			if ((flag & X500DistinguishedNameFlags.UseNewLines) != 0)
				return Environment.NewLine;
			return ", "; //default
		}

		// decode the DN using the (byte[]) RawData
		private void DecodeRawData ()
		{
			if ((RawData == null) || (RawData.Length < 3)) {
				name = String.Empty;
				return;
			}

			ASN1 sequence = new ASN1 (RawData);
			name = MX.X501.ToString (sequence, true, ", ", true);
		}
	}
}

#endif
