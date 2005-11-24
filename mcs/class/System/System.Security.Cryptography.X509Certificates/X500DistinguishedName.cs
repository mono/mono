//
// System.Security.Cryptography.X509Certificates.X500DistinguishedName
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

	public sealed class X500DistinguishedName : AsnEncodedData {

		private const X500DistinguishedNameFlags AllFlags = X500DistinguishedNameFlags.Reversed |
			X500DistinguishedNameFlags.UseSemicolons | X500DistinguishedNameFlags.DoNotUsePlusSign | 
			X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseCommas | 
			X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding | 
			X500DistinguishedNameFlags.UseT61Encoding | X500DistinguishedNameFlags.ForceUTF8Encoding;

		private string name;
		private ArrayList list;

		[MonoTODO]
		public X500DistinguishedName (AsnEncodedData encodedDistinguishedName)
		{
			if (encodedDistinguishedName == null)
				throw new ArgumentNullException ("encodedDistinguishedName");
			RawData = encodedDistinguishedName.RawData;
			DecodeRawData ();
			name = Decode (X500DistinguishedNameFlags.None);
		}

		[MonoTODO]
		public X500DistinguishedName (byte[] encodedDistinguishedName)
		{
			if (encodedDistinguishedName == null)
				throw new ArgumentNullException ("encodedDistinguishedName");
			Oid = new Oid ();
			RawData = encodedDistinguishedName;
			DecodeRawData ();
			name = Decode (X500DistinguishedNameFlags.None);
		}

		[MonoTODO]
		public X500DistinguishedName (string distinguishedName)
		{
			if (distinguishedName == null)
				throw new ArgumentNullException ("distinguishedName");

			if (distinguishedName.Length == 0) {
				// empty (0x00) ASN.1 sequence (0x30)
				RawData = new byte [2] { 0x30, 0x00 };
				DecodeRawData ();
			} else {
				DecodeName ();
				name = distinguishedName;
			}
		}

		[MonoTODO]
		public X500DistinguishedName (string distinguishedName, X500DistinguishedNameFlags flag)
		{
			if (distinguishedName == null)
				throw new ArgumentNullException ("distinguishedName");
			if ((flag != 0) && ((flag & AllFlags) == 0))
				throw new ArgumentException ("flag");

			if (distinguishedName.Length == 0) {
				// empty (0x00) ASN.1 sequence (0x30)
				RawData = new byte [2] { 0x30, 0x00 };
				DecodeRawData ();
			} else {
				DecodeName ();
				name = distinguishedName;
			}
		}

		[MonoTODO]
		public X500DistinguishedName (X500DistinguishedName distinguishedName)
		{
			if (distinguishedName == null)
				throw new ArgumentNullException ("distinguishedName");
			name = distinguishedName.name;
			list = (ArrayList) distinguishedName.list.Clone ();
		}

		[MonoTODO]
		public string Name {
			get { return name; }
		}

		[MonoTODO]
		public string Decode (X500DistinguishedNameFlags flag)
		{
			return String.Empty;
		}

		[MonoTODO]
		public override string Format (bool multiLine)
		{
			if (list.Count == 0)
				return String.Empty;

			StringBuilder sb = new StringBuilder ();
			foreach (DictionaryEntry de in list) {
				FormatEntry (sb, de, X500DistinguishedNameFlags.None);
				if (multiLine)
					sb.Append (Environment.NewLine);
			}
			if (multiLine)
				sb.Append (Environment.NewLine);
			return sb.ToString ();
		}

		// private stuff

		private void FormatEntry (StringBuilder sb, DictionaryEntry de, X500DistinguishedNameFlags flag)
		{
			sb.Append (de.Key);
			sb.Append ("=");
			// needs quotes ?
		}

		private string GetSeparator (X500DistinguishedNameFlags flag)
		{
			if ((flag & X500DistinguishedNameFlags.UseSemicolons) != 0)
				return ";";
			if ((flag & X500DistinguishedNameFlags.UseCommas) != 0)
				return ",";
			if ((flag & X500DistinguishedNameFlags.UseNewLines) != 0)
				return Environment.NewLine;
			return ","; //default
		}

		// decode the DN using the (byte[]) RawData
		private void DecodeRawData ()
		{
			list = new ArrayList ();
			if ((RawData == null) || (RawData.Length < 3)) {
				name = String.Empty;
				return;
			}

			ASN1 sequence = new ASN1 (RawData);
			for (int i=0; i < sequence.Count; i++) {
			}
		}

		// decode the DN using the (string) name
		private void DecodeName ()
		{
			if ((name == null) || (name.Length == 0))
				return;

			ASN1 dn = MX.X501.FromString (name);

			int pos = 0;
			ASN1 asn1 = new ASN1 (0x30);
/*			while (pos < name.Length) {
				MX.X520.AttributeTypeAndValue atv = ReadAttribute (name, ref pos);
				atv.Value = ReadValue (name, ref pos);
			}*/

			RawData = dn.GetBytes ();
			DecodeRawData ();
		}
	}
}

#endif
