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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace System.IO.Packaging {

	public class PackageDigitalSignature
	{
		internal PackageDigitalSignature ()
		{
		}

		public CertificateEmbeddingOption CertificateEmbeddingOption {
			get { throw new NotImplementedException (); }
		}

#if notyet
		public System.Security.Cryptography.Xml.Signature Signature {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif

		public PackagePart SignaturePart {
			get { throw new NotImplementedException (); }
		}

		public string SignatureType {
			get { throw new NotImplementedException (); }
		}

		public byte[] SignatureValue {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection<Uri> SignedParts {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection<PackageRelationshipSelector> SignedRelationshipSelectors {
			get { throw new NotImplementedException (); }
		}

		public X509Certificate Signer {
			get { throw new NotImplementedException (); }
		}

		public DateTime SigningTime {
			get { throw new NotImplementedException (); }
		}

		public string TimeFormat {
			get { throw new NotImplementedException (); }
		}

		public List<string> GetPartTransformList(Uri partName)
		{
			throw new NotImplementedException ();
		}

		public VerifyResult Verify()
		{
			throw new NotImplementedException ();
		}

		[SecurityCritical]
		[SecurityTreatAsSafe]
		public VerifyResult Verify(X509Certificate signingCertificate)
		{
			throw new NotImplementedException ();
		}

	}
}
