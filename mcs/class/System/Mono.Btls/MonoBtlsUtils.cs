//
// MonoBtlsUtils.cs
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
#if SECURITY_DEP && MONO_FEATURE_BTLS
using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Btls
{
	static class MonoBtlsUtils
	{
		static byte[] emailOid = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x09, 0x01 };

		public static bool Compare (byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; i++) {
				if (a[i] != b[i])
					return false;
			}
			return true;
		}

		static bool AppendEntry (StringBuilder sb, MonoBtlsX509Name name, int index, string separator, bool quotes)
		{
			var type = name.GetEntryType (index);
			if (type < 0)
				return false;
			else if (type == 0) {
				var oidValue = name.GetEntryOidData (index);
				if (Compare (oidValue, emailOid))
					type = MonoBtlsX509NameEntryType.Email;
			}
			int tag;
			var text = name.GetEntryValue (index, out tag);
			if (text == null)
				return false;
			var oid = name.GetEntryOid (index);
			if (oid == null)
				return false;

			if (sb.Length > 0)
				sb.Append (separator);

			switch (type) {
			case MonoBtlsX509NameEntryType.CountryName:
				sb.Append ("C=");
				break;
			case MonoBtlsX509NameEntryType.OrganizationName:
				sb.Append ("O=");
				break;
			case MonoBtlsX509NameEntryType.OrganizationalUnitName:
				sb.Append ("OU=");
				break;
			case MonoBtlsX509NameEntryType.CommonName:
				sb.Append ("CN=");
				break;
			case MonoBtlsX509NameEntryType.LocalityName:
				sb.Append ("L=");
				break;
			case MonoBtlsX509NameEntryType.StateOrProvinceName:
				sb.Append ("S=");       // NOTE: RFC2253 uses ST=
				break;
			case MonoBtlsX509NameEntryType.StreetAddress:
				sb.Append ("STREET=");
				break;
			case MonoBtlsX509NameEntryType.DomainComponent:
				sb.Append ("DC=");
				break;
			case MonoBtlsX509NameEntryType.UserId:
				sb.Append ("UID=");
				break;
			case MonoBtlsX509NameEntryType.Email:
				sb.Append ("E=");       // NOTE: Not part of RFC2253
				break;
			case MonoBtlsX509NameEntryType.DnQualifier:
				sb.Append ("dnQualifier=");
				break;
			case MonoBtlsX509NameEntryType.Title:
				sb.Append ("T=");
				break;
			case MonoBtlsX509NameEntryType.Surname:
				sb.Append ("SN=");
				break;
			case MonoBtlsX509NameEntryType.GivenName:
				sb.Append ("G=");
				break;
			case MonoBtlsX509NameEntryType.Initial:
				sb.Append ("I=");
				break;
			case MonoBtlsX509NameEntryType.SerialNumber:
				sb.Append ("SERIALNUMBER=");
				break;
			default:
				// unknown OID
				sb.Append ("OID.");     // NOTE: Not present as RFC2253
				sb.Append (oid);
				sb.Append ("=");
				break;
			}

			// 16bits or 8bits string ? TODO not complete (+special chars!)
			char[] specials = { ',', '+', '"', '\\', '<', '>', ';' };
			if (quotes && tag != 0x1E) {
				if ((text.IndexOfAny (specials, 0, text.Length) > 0) ||
				    text.StartsWith (" ") || (text.EndsWith (" ")))
					text = "\"" + text + "\"";
			}

			sb.Append (text);
			return true;
		}

		const X500DistinguishedNameFlags AllFlags = X500DistinguishedNameFlags.Reversed |
			X500DistinguishedNameFlags.UseSemicolons | X500DistinguishedNameFlags.DoNotUsePlusSign |
			X500DistinguishedNameFlags.DoNotUseQuotes | X500DistinguishedNameFlags.UseCommas |
			X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.UseUTF8Encoding |
			X500DistinguishedNameFlags.UseT61Encoding | X500DistinguishedNameFlags.ForceUTF8Encoding;

		static string GetSeparator (X500DistinguishedNameFlags flag)
		{
			if ((flag & X500DistinguishedNameFlags.UseSemicolons) != 0)
				return "; ";
			if ((flag & X500DistinguishedNameFlags.UseCommas) != 0)
				return ", ";
			if ((flag & X500DistinguishedNameFlags.UseNewLines) != 0)
				return Environment.NewLine;
			return ", "; //default
		}

		public static string FormatName (MonoBtlsX509Name name, X500DistinguishedNameFlags flag)
		{
			if ((flag != 0) && ((flag & AllFlags) == 0))
				throw new ArgumentException ("flag");

			if (name.GetEntryCount () == 0)
				return String.Empty;

			// Mono.Security reversed isn't the same as fx 2.0 (which is the reverse of 1.x)
			bool reversed = ((flag & X500DistinguishedNameFlags.Reversed) != 0);
			bool quotes = ((flag & X500DistinguishedNameFlags.DoNotUseQuotes) == 0);
			string separator = GetSeparator (flag);

			return FormatName (name, reversed, separator, quotes);
		}

		public static string FormatName (MonoBtlsX509Name name, bool reversed, string separator, bool quotes)
		{
			var count = name.GetEntryCount ();
			StringBuilder sb = new StringBuilder ();

			if (reversed) {
				for (int i = count - 1; i >= 0; i--) {
					AppendEntry (sb, name, i, separator, quotes);
				}
			} else {
				for (int i = 0; i < count; i++) {
					AppendEntry (sb, name, i, separator, quotes);
				}
			}

			return sb.ToString ();
		}
	}
}
#endif
