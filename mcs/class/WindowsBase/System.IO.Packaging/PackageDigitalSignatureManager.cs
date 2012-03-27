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
#if notyet
using System.Security.Cryptography.Xml;
#endif

namespace System.IO.Packaging {

	public sealed class PackageDigitalSignatureManager
	{
		public static string DefaultHashAlgorithm {
			get { throw new NotImplementedException (); }
		}

		public static string SignatureOriginRelationshipType {
			get { throw new NotImplementedException (); }
		}


		public PackageDigitalSignatureManager (Package package)
		{
			throw new NotImplementedException ();
		}


		public CertificateEmbeddingOption CertificateOption {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string HashAlgorithm {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public bool IsSigned {
			get { throw new NotImplementedException (); }
		}

		public IntPtr ParentWindow {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Uri SignatureOrigin {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection<PackageDigitalSignature> Signatures {
			get { throw new NotImplementedException (); }
		}

		public string TimeFormat {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public Dictionary<string, string> TransformMapping {
			get { throw new NotImplementedException (); }
		}


		public event InvalidSignatureEventHandler InvalidSignatureEvent;

		public PackageDigitalSignature Countersign()
		{
			throw new NotImplementedException ();
		}

		public PackageDigitalSignature Countersign(X509Certificate certificate)
		{
			throw new NotImplementedException ();
		}

		public PackageDigitalSignature Countersign(X509Certificate certificate, IEnumerable<Uri> signatures)
		{
			throw new NotImplementedException ();
		}

		public PackageDigitalSignature GetSignature (Uri signatureUri)
		{
			throw new NotImplementedException ();
		}


		public void RemoveSignature (Uri signatureUri)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAllSignatures ()
		{
			throw new NotImplementedException ();
		}


		public PackageDigitalSignature Sign (IEnumerable<Uri> parts)
		{
			throw new NotImplementedException ();
		}

		public PackageDigitalSignature Sign (IEnumerable<Uri> parts, X509Certificate certificate)
		{
			throw new NotImplementedException ();
		}

		public PackageDigitalSignature Sign (IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors)
		{
			throw new NotImplementedException ();
		}

		public PackageDigitalSignature Sign (IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors,
						     string signatureId)
		{
			throw new NotImplementedException ();
		}

#if notyet
		public PackageDigitalSignature Sign (IEnumerable<Uri> parts, X509Certificate certificate, IEnumerable<PackageRelationshipSelector> relationshipSelectors,
						     string signatureId,
						     IEnumerable<DataObject> signatureObjects,
						     IEnumerable<Reference> objectReferences)
		{
			throw new NotImplementedException ();
		}
#endif

		[SecurityCritical]
		public static X509ChainStatusFlags VerifyCertificate (X509Certificate certificate)
		{
			throw new NotImplementedException ();
		}

		public VerifyResult VerifySignatures (bool exitOnFailure)
		{
			throw new NotImplementedException ();
		}
	}

}
