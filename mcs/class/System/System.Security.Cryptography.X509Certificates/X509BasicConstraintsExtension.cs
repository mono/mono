//
// System.Security.Cryptography.X509BasicConstraintsExtension
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
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

	public sealed class X509BasicConstraintsExtension : X509Extension {

		internal const string oid = "2.5.29.19";
		internal const string friendlyName = "Basic Constraints";

		private bool _certificateAuthority;
		private bool _hasPathLengthConstraint;
		private int _pathLengthConstraint;
		private AsnDecodeStatus _status;

		// constructors

		public X509BasicConstraintsExtension ()
		{
			_oid = new Oid (oid, friendlyName);
		}

		public X509BasicConstraintsExtension (AsnEncodedData encodedBasicConstraints, bool critical)
		{
			// ignore the Oid provided by encodedKeyUsage (our rules!)
			_oid = new Oid (oid, friendlyName);
			_raw = encodedBasicConstraints.RawData;
			base.Critical = critical;
			_status = Decode (this.RawData);
		}

		public X509BasicConstraintsExtension (bool certificateAuthority, bool hasPathLengthConstraint, int pathLengthConstraint, bool critical)
		{
			if (hasPathLengthConstraint) {
				if (pathLengthConstraint < 0)
					throw new ArgumentOutOfRangeException ("pathLengthConstraint");
				_pathLengthConstraint = pathLengthConstraint;
			}
			_hasPathLengthConstraint = hasPathLengthConstraint;
			_certificateAuthority = certificateAuthority;
			_oid = new Oid (oid, friendlyName);
			base.Critical = critical;
			RawData = Encode ();
		}

		// properties

		public bool CertificateAuthority {
			get {
				switch (_status) {
				case AsnDecodeStatus.Ok:
				case AsnDecodeStatus.InformationNotAvailable:
					return _certificateAuthority;
				default:
					throw new CryptographicException ("Badly encoded extension.");
				}
			}
		}

		public bool HasPathLengthConstraint {
			get {
				switch (_status) {
				case AsnDecodeStatus.Ok:
				case AsnDecodeStatus.InformationNotAvailable:
					return _hasPathLengthConstraint;
				default:
					throw new CryptographicException ("Badly encoded extension.");
				}
			}
		}

		public int PathLengthConstraint {
			get {
				switch (_status) {
				case AsnDecodeStatus.Ok:
				case AsnDecodeStatus.InformationNotAvailable:
					return _pathLengthConstraint;
				default:
					throw new CryptographicException ("Badly encoded extension.");
				}
			}
		}

		// methods

		public override void CopyFrom (AsnEncodedData asnEncodedData) 
		{
			if (asnEncodedData == null)
				throw new ArgumentNullException ("asnEncodedData");

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
			if (extension.Length < 3) {
				if (!((extension.Length == 2) && (extension [1] == 0x00)))
					return AsnDecodeStatus.BadLength;
			}

			try {
				ASN1 sequence = new ASN1 (extension);
				int n = 0;
				ASN1 a = sequence [n++];
				if ((a != null) && (a.Tag == 0x01)) {
					_certificateAuthority = (a.Value [0] == 0xFF);
					a = sequence [n++];
				}
				if ((a != null) && (a.Tag == 0x02)) {
					_hasPathLengthConstraint = true;
					_pathLengthConstraint = ASN1Convert.ToInt32 (a);
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

			if (_certificateAuthority)
				ex.Add (new ASN1 (0x01, new byte[] { 0xFF }));
			if (_hasPathLengthConstraint) {
				// MS encodes the 0 (pathLengthConstraint is OPTIONAL)
				// and in a long form (02 00 versus 02 01 00)
				if (_pathLengthConstraint == 0)
					ex.Add (new ASN1 (0x02, new byte[] { 0x00 }));
				else
					ex.Add (ASN1Convert.FromInt32 (_pathLengthConstraint));
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

			StringBuilder sb = new StringBuilder ();

			sb.Append ("Subject Type=");
			if (_certificateAuthority)
				sb.Append ("CA");
			else
				sb.Append ("End Entity");
			if (multiLine)
				sb.Append (Environment.NewLine);
			else
				sb.Append (", ");

			sb.Append ("Path Length Constraint=");
			if (_hasPathLengthConstraint) 
				sb.Append (_pathLengthConstraint);
			else
				sb.Append ("None");
			if (multiLine)
				sb.Append (Environment.NewLine);

			return sb.ToString ();
		}
	}
}

#endif
