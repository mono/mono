//
// AsnEncodedData.cs - System.Security.Cryptography.AsnEncodedData
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
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

#if SECURITY_DEP || MOONLIGHT

using System.Security.Cryptography.X509Certificates;
using System.Text;

using Mono.Security;
using Mono.Security.Cryptography;

namespace System.Security.Cryptography {

	internal enum AsnDecodeStatus {
		NotDecoded = -1,
		Ok = 0,
		BadAsn = 1,
		BadTag = 2,
		BadLength = 3,
		InformationNotAvailable = 4
	}

	public class AsnEncodedData {

		internal Oid _oid;
		internal byte[] _raw;

		// constructors

		protected AsnEncodedData ()
		{
		}
	
		public AsnEncodedData (string oid, byte[] rawData)
		{
			_oid = new Oid (oid);
			RawData = rawData;
		}

		public AsnEncodedData (Oid oid, byte[] rawData)
		{
			Oid = oid;
			RawData = rawData;

			// yes, here oid == null is legal (by design), 
			// but no, it would not be legal for an oid string
			// see MSDN FDBK11479
		}

		public AsnEncodedData (AsnEncodedData asnEncodedData)
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("asnEncodedData");

			if (asnEncodedData._oid != null)
				Oid = new Oid (asnEncodedData._oid);
			RawData = asnEncodedData._raw;
		}

		public AsnEncodedData (byte[] rawData)
		{
			RawData = rawData;
		}

		// properties

		public Oid Oid {
			get { return _oid; }
			set {
				if (value == null)
					_oid = null;
				else
					_oid = new Oid (value);
			}
		}

		public byte[] RawData { 
			get { return _raw; }
			set {
				if (value == null)
					throw new ArgumentNullException ("RawData");
				_raw = (byte[])value.Clone ();
			}
		}

		// methods

		public virtual void CopyFrom (AsnEncodedData asnEncodedData)
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("asnEncodedData");

			if (asnEncodedData._oid == null)
				Oid = null;
			else
				Oid = new Oid (asnEncodedData._oid);

			RawData = asnEncodedData._raw;
		}

		public virtual string Format (bool multiLine) 
		{
			if (_raw == null)
				return String.Empty;

			if (_oid == null)
				return Default (multiLine);

			return ToString (multiLine);
		}

		// internal decoding/formatting methods

		internal virtual string ToString (bool multiLine)
		{
			switch (_oid.Value) {
			// fx supported objects
			case X509BasicConstraintsExtension.oid:
				return BasicConstraintsExtension (multiLine);
			case X509EnhancedKeyUsageExtension.oid:
				return EnhancedKeyUsageExtension (multiLine);
			case X509KeyUsageExtension.oid:
				return KeyUsageExtension (multiLine);
			case X509SubjectKeyIdentifierExtension.oid:
				return SubjectKeyIdentifierExtension (multiLine);
			// other known objects (i.e. supported structure) - 
			// but without any corresponding framework class
			case Oid.oidSubjectAltName:
				return SubjectAltName (multiLine);
			case Oid.oidNetscapeCertType:
				return NetscapeCertType (multiLine);
			default:
				return Default (multiLine);
			}
		}

		internal string Default (bool multiLine)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i=0; i < _raw.Length; i++) {
				sb.Append (_raw [i].ToString ("x2"));
				if (i != _raw.Length - 1)
					sb.Append (" ");
			}
			return sb.ToString ();
		}

		// Indirectly (undocumented but) supported extensions

		internal string BasicConstraintsExtension (bool multiLine)
		{
			try {
				X509BasicConstraintsExtension bc = new X509BasicConstraintsExtension  (this, false);
				return bc.ToString (multiLine);
			}
			catch {
				return String.Empty;
			}
		}

		internal string EnhancedKeyUsageExtension (bool multiLine)
		{
			try {
				X509EnhancedKeyUsageExtension eku = new X509EnhancedKeyUsageExtension  (this, false);
				return eku.ToString (multiLine);
			}
			catch {
				return String.Empty;
			}
		}

		internal string KeyUsageExtension (bool multiLine)
		{
			try {
				X509KeyUsageExtension ku = new X509KeyUsageExtension  (this, false);
				return ku.ToString (multiLine);
			}
			catch {
				return String.Empty;
			}
		}

		internal string SubjectKeyIdentifierExtension (bool multiLine)
		{
			try {
				X509SubjectKeyIdentifierExtension ski = new X509SubjectKeyIdentifierExtension  (this, false);
				return ski.ToString (multiLine);
			}
			catch {
				return String.Empty;
			}
		}

		// Indirectly (undocumented but) supported extensions

		internal string SubjectAltName (bool multiLine)
		{
			if (_raw.Length < 5)
				return "Information Not Available";

			try {
				ASN1 ex = new ASN1 (_raw);
				StringBuilder sb = new StringBuilder ();
				for (int i=0; i < ex.Count; i++) {
					ASN1 el = ex [i];

					string type = null;
					string name = null;

					switch (el.Tag) {
					case 0x81:
						type = "RFC822 Name=";
						name = Encoding.ASCII.GetString (el.Value);
						break;
					case 0x82:
						type = "DNS Name=";
						name = Encoding.ASCII.GetString (el.Value);
						break;
					default:
						type = String.Format ("Unknown ({0})=", el.Tag);
						name = CryptoConvert.ToHex (el.Value);
						break;
					}

					sb.Append (type);
					sb.Append (name);
					if (multiLine) {
						sb.Append (Environment.NewLine);
					} else if (i < ex.Count - 1) {
						sb.Append (", ");
					}
				}
				return sb.ToString ();
			}
			catch {
				return String.Empty;
			}
		}

		internal string NetscapeCertType (bool multiLine)
		{
			// 4 byte long, BITSTRING (0x03), Value length of 2
			if ((_raw.Length < 4) || (_raw [0] != 0x03) || (_raw [1] != 0x02))
				return "Information Not Available";
			// first value byte is the number of unused bits
			int value = (_raw [3] >> _raw [2]) << _raw [2];

			StringBuilder sb = new StringBuilder ();

			if ((value & 0x80) == 0x80) {
				sb.Append ("SSL Client Authentication");
			}
			if ((value & 0x40) == 0x40) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("SSL Server Authentication");
			}
			if ((value & 0x20) == 0x20) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("SMIME");
			}
			if ((value & 0x10) == 0x10) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Signature"); // a.k.a. Object Signing / Code Signing
			}
			if ((value & 0x08) == 0x08) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Unknown cert type");
			}
			if ((value & 0x04) == 0x04) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("SSL CA");	// CA == Certificate Authority
			}
			if ((value & 0x02) == 0x02) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("SMIME CA");
			}
			if ((value & 0x01) == 0x01) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Signature CA");
			}
			sb.AppendFormat (" ({0})", value.ToString ("x2"));
			return sb.ToString ();
		}
	}
}

#endif
