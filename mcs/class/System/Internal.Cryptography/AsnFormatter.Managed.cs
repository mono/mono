//
// Copyright (c) 2018 Microsoft
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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Internal.Cryptography
{
	internal abstract partial class AsnFormatter
	{
		private static readonly AsnFormatter s_instance = new ManagedAsnFormatter ();
	}

	/// <summary>
	/// ASN.1 Formatter that closely resembles the output on .NET Framework on Windows.
	/// </summary>
	internal class ManagedAsnFormatter : AsnFormatter
	{
		protected override string FormatNative(Oid oid, byte[] rawData, bool multiLine)
		{
			switch (oid?.Value) {
			case "2.5.29.14":
				return FormatSubjectKeyIdentifierName (rawData, multiLine);
			case "2.5.29.15":
				return FormatKeyUsageName (rawData, multiLine);
			case "2.5.29.17":
				return FormatSubjectAlternativeName (rawData, multiLine);
			case "2.5.29.19":
				return FormatBasicConstraintsName (rawData, multiLine);
			case "2.5.29.37":
				return FormatEnhancedKeyUsageName (rawData, multiLine);
			case "2.16.840.1.113730.1.1":
				return FormatNetscapeCertName (rawData, multiLine);
			}
			return EncodeHexCompat (rawData, false);
		}

		private string EncodeHexCompat (byte[] rawData, bool multiLine)
		{
 			StringBuilder sb = new StringBuilder ();
		   	for (int i = 0; i < rawData.Length; i++) {
				sb.Append (rawData [i].ToString ("x2"));
				if (i != rawData.Length - 1)
					sb.Append (" ");
			}
					
			if (multiLine)
				sb.AppendLine ();

			return sb.ToString ();
		}

		private string FormatSubjectKeyIdentifierName (byte[] rawData, bool multiLine)
		{
			DerSequenceReader skiReader = DerSequenceReader.CreateForPayload (rawData);
			return EncodeHexCompat (skiReader.ReadOctetString (), multiLine);
		}

		private string FormatKeyUsageName (byte[] rawData, bool multiLine)
		{
			DerSequenceReader kuReader = DerSequenceReader.CreateForPayload (rawData);
			var keyUsage = kuReader.ReadBitString ();
			int kubits = 0;
			for (int i = 0; i < keyUsage.Length; i++)
				kubits += keyUsage [i] << (i * 8);

			if (kubits == 0)
				return "Information Not Available";

			StringBuilder sb = new StringBuilder ();
			X509KeyUsageFlags flags = (X509KeyUsageFlags) kubits;
			List<string> usageNames = new List<string> ();

			if ((flags & X509KeyUsageFlags.DigitalSignature) != 0) {
				usageNames.Add ("Digital Signature");
			}
			if ((flags & X509KeyUsageFlags.NonRepudiation) != 0) {
				usageNames.Add ("Non-Repudiation");
			}
			if ((flags & X509KeyUsageFlags.KeyEncipherment) != 0) {
				usageNames.Add ("Key Encipherment");
			}
			if ((flags & X509KeyUsageFlags.DataEncipherment) != 0) {
				usageNames.Add ("Data Encipherment");
			}
			if ((flags & X509KeyUsageFlags.KeyAgreement) != 0) {
				usageNames.Add ("Key Agreement");
			}
			if ((flags & X509KeyUsageFlags.KeyCertSign) != 0) {
				usageNames.Add ("Certificate Signing");
			}
			if ((flags & X509KeyUsageFlags.CrlSign) != 0) {
				usageNames.Add ("Off-line CRL Signing, CRL Signing");
			}
			if ((flags & X509KeyUsageFlags.EncipherOnly) != 0) {
				usageNames.Add ("Encipher Only");
			}
			if ((flags & X509KeyUsageFlags.DecipherOnly) != 0) {
				usageNames.Add ("Decipher Only");
			}

			sb.Append (String.Join (", ", usageNames));
			sb.Append (" (");
			sb.Append (((byte)kubits).ToString ("x2"));
			if (kubits > System.Byte.MaxValue) {
				sb.Append (" ");
				sb.Append (((byte)(kubits >> 8)).ToString ("x2"));
			}
			sb.Append (")");

			if (multiLine)
				sb.AppendLine ();

			return sb.ToString ();
		}

		internal enum GeneralNameType
		{
			Rfc822Name = 1,
			DnsName = 2,
		}

		private string FormatSubjectAlternativeName (byte[] rawData, bool multiLine)
		{
			DerSequenceReader altNameReader = new DerSequenceReader (rawData);
			StringBuilder sb = new StringBuilder ();

			while (altNameReader.HasData) {
				if (!multiLine && sb.Length > 0) {
					sb.Append (", ");
				}

				byte tag = altNameReader.PeekTag ();
				switch ((GeneralNameType)(tag & DerSequenceReader.TagNumberMask)) {
				case GeneralNameType.Rfc822Name:
					sb.Append ("RFC822 Name=");
					sb.Append (altNameReader.ReadIA5String ());
					break;
				case GeneralNameType.DnsName:
					sb.Append ("DNS Name=");
					sb.Append (altNameReader.ReadIA5String ());
					break;
				default:
					sb.Append (String.Format ("Unknown ({0})=", tag));
					sb.Append (EncodeHexCompat (altNameReader.ReadNextEncodedValue (), false));
					break;
				}

				if (multiLine) {
					sb.AppendLine ();
				}
			}

			return sb.ToString();
		}

		private string FormatBasicConstraintsName (byte[] rawData, bool multiLine)
		{
			DerSequenceReader constraintsReader = new DerSequenceReader (rawData);
			StringBuilder sb = new StringBuilder ();
			bool certificateAuthority = false;
			bool hasPathLengthConstraint = false;
			int pathLengthConstraint = 0;

			while (constraintsReader.HasData) {
				byte tag = constraintsReader.PeekTag ();
				if (tag == 1) {
					certificateAuthority = constraintsReader.ReadBoolean ();
				} else if (tag == 2) {
					hasPathLengthConstraint = true;
					pathLengthConstraint = constraintsReader.ReadInteger ();
				} else {
					constraintsReader.SkipValue ();
				}
			}

			sb.Append ("Subject Type=");
			if (certificateAuthority)
				sb.Append ("CA");
			else
				sb.Append ("End Entity");
			if (multiLine)
				sb.AppendLine ();
			else
				sb.Append (", ");

			sb.Append ("Path Length Constraint=");
			if (hasPathLengthConstraint) 
				sb.Append (pathLengthConstraint);
			else
				sb.Append ("None");
			if (multiLine)
				sb.AppendLine ();

			return sb.ToString ();
		}

		private string FormatEnhancedKeyUsageName (byte[] rawData, bool multiLine)
		{
			DerSequenceReader ekuReader = new DerSequenceReader (rawData);
			StringBuilder sb = new StringBuilder ();

			while (ekuReader.HasData) {
				if (!multiLine && sb.Length > 0)
					sb.Append (", ");

				string oid = ekuReader.ReadOidAsString();
				switch (oid) {
				case "1.3.6.1.5.5.7.3.1":
					sb.Append ("Server Authentication (");
					break;
				case "1.3.6.1.5.5.7.3.2":
					sb.Append ("Client Authentication (");
					break;
				case "1.3.6.1.5.5.7.3.3":
					sb.Append ("Code Signing (");
					break;
				case "1.3.6.1.5.5.7.3.4":
					sb.Append ("E-mail Protection (");
					break;
				default:
					sb.Append ("Unknown Key Usage (");
					break;
				}
				sb.Append (oid);
				sb.Append (")");

				if (multiLine)
					sb.Append (Environment.NewLine);
			}

			if (sb.Length == 0)
				return "Information Not Available";

			return sb.ToString ();
		}

		private string FormatNetscapeCertName (byte[] rawData, bool multiLine)
		{
			// 4 byte long, BITSTRING (0x03), Value length of 2
			if ((rawData.Length < 4) || (rawData [0] != 0x03) || (rawData [1] != 0x02))
				return "Information Not Available";
			// first value byte is the number of unused bits
			int value = (rawData [3] >> rawData [2]) << rawData [2];

			StringBuilder sb = new StringBuilder ();
			List<string> usageNames = new List<string> ();

			if ((value & 0x80) == 0x80) {
				usageNames.Add ("SSL Client Authentication");
			}
			if ((value & 0x40) == 0x40) {
				usageNames.Add ("SSL Server Authentication");
			}
			if ((value & 0x20) == 0x20) {
				usageNames.Add ("SMIME");
			}
			if ((value & 0x10) == 0x10) {
				usageNames.Add ("Signature"); // a.k.a. Object Signing / Code Signing
			}
			if ((value & 0x08) == 0x08) {
				usageNames.Add ("Unknown cert type");
			}
			if ((value & 0x04) == 0x04) {
				usageNames.Add ("SSL CA");
			}
			if ((value & 0x02) == 0x02) {
				usageNames.Add ("SMIME CA");
			}
			if ((value & 0x01) == 0x01) {
				usageNames.Add ("Signature CA");
			}

			sb.Append (String.Join (", ", usageNames));
			sb.AppendFormat (" ({0})", value.ToString ("x2"));

			return sb.ToString ();
		}
	}
}
