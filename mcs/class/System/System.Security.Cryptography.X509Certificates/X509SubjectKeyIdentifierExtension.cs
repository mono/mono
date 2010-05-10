//
// System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension
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
using Mono.Security.Cryptography;

namespace System.Security.Cryptography.X509Certificates {

	public sealed class X509SubjectKeyIdentifierExtension : X509Extension {

		internal const string oid = "2.5.29.14";
		internal const string friendlyName = "Subject Key Identifier";

		private byte[] _subjectKeyIdentifier;
		private string _ski;
		private AsnDecodeStatus _status;

		// constructors

		public X509SubjectKeyIdentifierExtension ()
		{
			_oid = new Oid (oid, friendlyName);
		}

		public X509SubjectKeyIdentifierExtension (AsnEncodedData encodedSubjectKeyIdentifier, bool critical)
		{
			// ignore the Oid provided by encodedKeyUsage (our rules!)
			_oid = new Oid (oid, friendlyName);
			_raw = encodedSubjectKeyIdentifier.RawData;
			base.Critical = critical;
			_status = Decode (this.RawData);
		}

		public X509SubjectKeyIdentifierExtension (byte[] subjectKeyIdentifier, bool critical)
		{
			if (subjectKeyIdentifier == null)
				throw new ArgumentNullException ("subjectKeyIdentifier");
			if (subjectKeyIdentifier.Length == 0)
				throw new ArgumentException ("subjectKeyIdentifier");

			_oid = new Oid (oid, friendlyName);
			base.Critical = critical;
			_subjectKeyIdentifier = (byte[])subjectKeyIdentifier.Clone ();
			RawData = Encode ();
		}

		public X509SubjectKeyIdentifierExtension (string subjectKeyIdentifier, bool critical)
		{
			if (subjectKeyIdentifier == null)
				throw new ArgumentNullException ("subjectKeyIdentifier");
			if (subjectKeyIdentifier.Length < 2)
				throw new ArgumentException ("subjectKeyIdentifier");

			_oid = new Oid (oid, friendlyName);
			base.Critical = critical;
			_subjectKeyIdentifier = FromHex (subjectKeyIdentifier);
			RawData = Encode ();
		}

		public X509SubjectKeyIdentifierExtension (PublicKey key, bool critical)
			: this (key, X509SubjectKeyIdentifierHashAlgorithm.Sha1, critical)
		{
		}

		public X509SubjectKeyIdentifierExtension (PublicKey key, X509SubjectKeyIdentifierHashAlgorithm algorithm, bool critical)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			byte[] pkraw = key.EncodedKeyValue.RawData;
			// compute SKI
			switch (algorithm) {
			// hash of the public key, excluding Tag, Length and unused bits values
			case X509SubjectKeyIdentifierHashAlgorithm.Sha1:
				_subjectKeyIdentifier = SHA1.Create ().ComputeHash (pkraw);
				break;
			// 0100 bit pattern followed by the 60 last bit of the hash
			case X509SubjectKeyIdentifierHashAlgorithm.ShortSha1:
				byte[] hash = SHA1.Create ().ComputeHash (pkraw);
				_subjectKeyIdentifier = new byte [8];
				Buffer.BlockCopy (hash, 12, _subjectKeyIdentifier, 0, 8);
				_subjectKeyIdentifier [0] = (byte) (0x40 | (_subjectKeyIdentifier [0] & 0x0F));
				break;
			// hash of the public key, including Tag, Length and unused bits values
			case X509SubjectKeyIdentifierHashAlgorithm.CapiSha1:
				// CryptoAPI does that hash on the complete subjectPublicKeyInfo (unlike PKIX)
				// http://groups.google.ca/groups?selm=e7RqM%24plCHA.1488%40tkmsftngp02&oe=UTF-8&output=gplain
				ASN1 subjectPublicKeyInfo = new ASN1 (0x30);
				ASN1 algo = subjectPublicKeyInfo.Add (new ASN1 (0x30));
				algo.Add (new ASN1 (CryptoConfig.EncodeOID (key.Oid.Value)));
				algo.Add (new ASN1 (key.EncodedParameters.RawData)); 
				// add an extra byte for the unused bits (none)
				byte[] full = new byte [pkraw.Length + 1];
				Buffer.BlockCopy (pkraw, 0, full, 1, pkraw.Length);
				subjectPublicKeyInfo.Add (new ASN1 (0x03, full));
				_subjectKeyIdentifier = SHA1.Create ().ComputeHash (subjectPublicKeyInfo.GetBytes ());
				break;
			default:
				throw new ArgumentException ("algorithm");
			}

			_oid = new Oid (oid, friendlyName);
			base.Critical = critical;
			RawData = Encode ();
		}

		// properties

		public string SubjectKeyIdentifier {
			get {
				switch (_status) {
				case AsnDecodeStatus.Ok:
				case AsnDecodeStatus.InformationNotAvailable:
					if (_subjectKeyIdentifier != null)
						_ski = CryptoConvert.ToHex (_subjectKeyIdentifier);
					return _ski;
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

		static internal byte FromHexChar (char c) 
		{
			if ((c >= 'a') && (c <= 'f'))
				return (byte) (c - 'a' + 10);
			if ((c >= 'A') && (c <= 'F'))
				return (byte) (c - 'A' + 10);
			if ((c >= '0') && (c <= '9'))
				return (byte) (c - '0');
			return 255;	// F
		}

		static internal byte FromHexChars (char c1, char c2)
		{
			byte result = FromHexChar (c1);
			if (result < 255)
				result = (byte) ((result << 4) | FromHexChar (c2));
			return result;
		}

		static internal byte[] FromHex (string hex)
		{
			// here we can't use CryptoConvert.FromHex because we
			// must convert any *illegal* (non hex) 2 characters 
			// to 'FF' and ignore last char on odd length
			if (hex == null)
				return null;

			int length = hex.Length >> 1;

			byte[] result = new byte [length]; // + (odd ? 1 : 0)];
			int n = 0;
			int i = 0;
			while (n < length) {
				result [n++] = FromHexChars (hex [i++], hex [i++]);
			}
			return result;
		}

		internal AsnDecodeStatus Decode (byte[] extension)
		{
			if ((extension == null) || (extension.Length == 0))
				return AsnDecodeStatus.BadAsn;
			_ski = String.Empty;
			if (extension [0] != 0x04)
				return AsnDecodeStatus.BadTag;
			if (extension.Length == 2)
				return AsnDecodeStatus.InformationNotAvailable;
			if (extension.Length < 3)
				return AsnDecodeStatus.BadLength;

			try {
				ASN1 ex = new ASN1 (extension);
				_subjectKeyIdentifier = ex.Value;
			}
			catch {
				return AsnDecodeStatus.BadAsn;
			}

			return AsnDecodeStatus.Ok;
		}

		internal byte[] Encode ()
		{
			ASN1 ex = new ASN1 (0x04, _subjectKeyIdentifier);
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

			StringBuilder sb = new StringBuilder ();

			for (int i=0; i < _subjectKeyIdentifier.Length; i++) {
				sb.Append (_subjectKeyIdentifier [i].ToString ("x2"));
				if (i != _subjectKeyIdentifier.Length - 1)
					sb.Append (" ");
			}

			if (multiLine)
				sb.Append (Environment.NewLine);

			return sb.ToString ();
		}
	}
}

#endif
