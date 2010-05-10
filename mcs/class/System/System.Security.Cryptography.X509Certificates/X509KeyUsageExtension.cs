//
// System.Security.Cryptography.X509Certificates.X509KeyUsageExtension
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2004
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

using System.Text;

using Mono.Security;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509KeyUsageExtension : X509Extension {

		internal const string oid = "2.5.29.15";
		internal const string friendlyName = "Key Usage";

		internal const X509KeyUsageFlags all = X509KeyUsageFlags.EncipherOnly | X509KeyUsageFlags.CrlSign | 
			X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.KeyAgreement | X509KeyUsageFlags.DataEncipherment |
			X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.NonRepudiation | 
			X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DecipherOnly;

		private X509KeyUsageFlags _keyUsages;
		private AsnDecodeStatus _status;

		// constructors

		public X509KeyUsageExtension ()
		{
			_oid = new Oid (oid, friendlyName);
		}

		public X509KeyUsageExtension (AsnEncodedData encodedKeyUsage, bool critical)
		{
			// ignore the Oid provided by encodedKeyUsage (our rules!)
			_oid = new Oid (oid, friendlyName);
			_raw = encodedKeyUsage.RawData;
			base.Critical = critical;
			_status = Decode (this.RawData);
		}

		public X509KeyUsageExtension (X509KeyUsageFlags keyUsages, bool critical)
		{
			_oid = new Oid (oid, friendlyName);
			base.Critical = critical;
			_keyUsages = GetValidFlags (keyUsages);
			RawData = Encode ();
		}

		// properties

		public X509KeyUsageFlags KeyUsages {
			get {
				switch (_status) {
				case AsnDecodeStatus.Ok:
				case AsnDecodeStatus.InformationNotAvailable:
					return _keyUsages;
				default:
					throw new CryptographicException ("Badly encoded extension.");
				}
			}
		}

		// methods

		public override void CopyFrom (AsnEncodedData encodedData)
		{
			if (encodedData == null)
				throw new ArgumentNullException ("encodedData");

			X509Extension ex = (encodedData as X509Extension);
			if (ex == null)
				throw new ArgumentException (Locale.GetText ("Wrong type."), "encodedData");

			if (ex._oid == null)
				_oid = new Oid (oid, friendlyName);
			else 
				_oid = new Oid (ex._oid);

			RawData = ex.RawData;
			base.Critical = ex.Critical;
			// and we deal with the rest later
			_status = Decode (this.RawData);
		}

		// internal

		internal X509KeyUsageFlags GetValidFlags (X509KeyUsageFlags flags)
		{
			if ((flags & all) != flags)
				return (X509KeyUsageFlags) 0;
			return flags;
		}

		internal AsnDecodeStatus Decode (byte[] extension)
		{
			if ((extension == null) || (extension.Length == 0))
				return AsnDecodeStatus.BadAsn;
			if (extension [0] != 0x03)
				return AsnDecodeStatus.BadTag;
			if (extension.Length < 3)
				return AsnDecodeStatus.BadLength;
			if (extension.Length < 4)
				return AsnDecodeStatus.InformationNotAvailable;

			try {
				ASN1 ex = new ASN1 (extension);
				int kubits = 0;
				int i = 1; // byte zero has the number of unused bits (ASN1's BITSTRING)
				while (i < ex.Value.Length)
					kubits = (kubits << 8) + ex.Value [i++];

				_keyUsages = GetValidFlags ((X509KeyUsageFlags)kubits);
			}
			catch {
				return AsnDecodeStatus.BadAsn;
			}

			return AsnDecodeStatus.Ok;
		}

		internal byte[] Encode ()
		{
			ASN1 ex = null;
			int kubits = (int)_keyUsages;
			byte empty = 0;

			if (kubits == 0) {
				ex = new ASN1 (0x03, new byte[] { empty });
			} else {
				// count empty bits (applicable to first byte only)
				int ku = ((kubits < Byte.MaxValue) ? kubits : (kubits >> 8));
				while (((ku & 0x01) == 0x00) && (empty < 8)) {
					empty++;
					ku >>= 1;
				}

				if (kubits <= Byte.MaxValue) {
					ex = new ASN1 (0x03, new byte[] { empty, (byte)kubits });
				} else {
					ex = new ASN1 (0x03, new byte[] { empty, (byte)kubits, (byte)(kubits >> 8) });
				}
			}

			return ex.GetBytes ();
		}

		internal override string ToString (bool multiLine)
		{
			switch (_status) {
			case AsnDecodeStatus.BadAsn:
				return String.Empty;
			case AsnDecodeStatus.BadTag:
			case AsnDecodeStatus.BadLength:
				return FormatUnkownData (_raw);
			case AsnDecodeStatus.InformationNotAvailable:
				return "Information Not Available";
			}

			if (_oid.Value != oid)
				return String.Format ("Unknown Key Usage ({0})", _oid.Value);
			if (_keyUsages == 0)
				return "Information Not Available";

			StringBuilder sb = new StringBuilder ();

			if ((_keyUsages & X509KeyUsageFlags.DigitalSignature) != 0) {
				sb.Append ("Digital Signature");
			}
			if ((_keyUsages & X509KeyUsageFlags.NonRepudiation) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Non-Repudiation");
			}
			if ((_keyUsages & X509KeyUsageFlags.KeyEncipherment) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Key Encipherment");
			}
			if ((_keyUsages & X509KeyUsageFlags.DataEncipherment) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Data Encipherment");
			}
			if ((_keyUsages & X509KeyUsageFlags.KeyAgreement) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Key Agreement");
			}
			if ((_keyUsages & X509KeyUsageFlags.KeyCertSign) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Certificate Signing");
			}
			if ((_keyUsages & X509KeyUsageFlags.CrlSign) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Off-line CRL Signing, CRL Signing");
			}
			if ((_keyUsages & X509KeyUsageFlags.EncipherOnly) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Encipher Only");
			}
			if ((_keyUsages & X509KeyUsageFlags.DecipherOnly) != 0) {
				if (sb.Length > 0)
					sb.Append (", ");
				sb.Append ("Decipher Only");
			}

			int ku = (int)_keyUsages;
			sb.Append (" (");
			sb.Append (((byte)ku).ToString ("x2"));
			if (ku > Byte.MaxValue) {
				sb.Append (" ");
				sb.Append (((byte)(ku >> 8)).ToString ("x2"));
			}
			sb.Append (")");

			if (multiLine)
				sb.Append (Environment.NewLine);

			return sb.ToString ();
		}
	}
}

#endif
