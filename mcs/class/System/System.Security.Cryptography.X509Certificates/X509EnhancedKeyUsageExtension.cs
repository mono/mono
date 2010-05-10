//
// System.Security.Cryptography.X509EnhancedKeyUsageExtension
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

	public sealed class X509EnhancedKeyUsageExtension : X509Extension {

		internal const string oid = "2.5.29.37";
		internal const string friendlyName = "Enhanced Key Usage";

		private OidCollection _enhKeyUsage;
		private AsnDecodeStatus _status;

		// constructors

		public X509EnhancedKeyUsageExtension ()
		{
			_oid = new Oid (oid, friendlyName);
		}

		public X509EnhancedKeyUsageExtension (AsnEncodedData encodedEnhancedKeyUsages, bool critical)
		{
			// ignore the Oid provided by encodedKeyUsage (our rules!)
			_oid = new Oid (oid, friendlyName);
			_raw = encodedEnhancedKeyUsages.RawData;
			base.Critical = critical;
			_status = Decode (this.RawData);
		}

		public X509EnhancedKeyUsageExtension (OidCollection enhancedKeyUsages, bool critical)
		{
			if (enhancedKeyUsages == null)
				throw new ArgumentNullException ("enhancedKeyUsages");

			_oid = new Oid (oid, friendlyName);
			base.Critical = critical;
			_enhKeyUsage = enhancedKeyUsages.ReadOnlyCopy ();
			RawData = Encode ();
		}

		// properties

		public OidCollection EnhancedKeyUsages {
			get {
				switch (_status) {
				case AsnDecodeStatus.Ok:
				case AsnDecodeStatus.InformationNotAvailable:
					if (_enhKeyUsage == null)
						_enhKeyUsage = new OidCollection ();
					_enhKeyUsage.ReadOnly = true;
					return _enhKeyUsage;
				default:
					throw new CryptographicException ("Badly encoded extension.");
				}
			}
		}

		// methods

		public override void CopyFrom (AsnEncodedData asnEncodedData) 
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("encodedData");

			X509Extension ex = (asnEncodedData as X509Extension);
			if (ex == null)
				throw new ArgumentException (Locale.GetText ("Wrong type."), "asnEncodedData");

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

		internal AsnDecodeStatus Decode (byte[] extension)
		{
			if ((extension == null) || (extension.Length == 0))
				return AsnDecodeStatus.BadAsn;
			if (extension [0] != 0x30)
				return AsnDecodeStatus.BadTag;

			if (_enhKeyUsage == null)
				_enhKeyUsage = new OidCollection ();

			try {
				ASN1 ex = new ASN1 (extension);
				if (ex.Tag != 0x30)
					throw new CryptographicException (Locale.GetText ("Invalid ASN.1 Tag"));
				for (int i=0; i < ex.Count; i++) {
					_enhKeyUsage.Add (new Oid (ASN1Convert.ToOid (ex [i])));
				}
			}
			catch {
				return AsnDecodeStatus.BadAsn;
			}

			return AsnDecodeStatus.Ok;
		}

		internal byte[] Encode ()
		{
			ASN1 ex = new ASN1 (0x30);
			foreach (Oid oid in _enhKeyUsage) {
				ex.Add (ASN1Convert.FromOid (oid.Value));
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
			if (_enhKeyUsage.Count == 0)
				return "Information Not Available";

			StringBuilder sb = new StringBuilder ();

			for (int i=0; i < _enhKeyUsage.Count; i++) {
				Oid o = _enhKeyUsage [i];
				switch (o.Value) {
				case "1.3.6.1.5.5.7.3.1":
					sb.Append ("Server Authentication (");
					break;
				default:
					sb.Append ("Unknown Key Usage (");
					break;
				}
				sb.Append (o.Value);
				sb.Append (")");

				if (multiLine)
					sb.Append (Environment.NewLine);
				else if (i != (_enhKeyUsage.Count - 1))
					sb.Append (", ");
			}

			return sb.ToString ();
		}
	}
}

#endif
