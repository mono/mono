//
// System.Security.Cryptography.Pkcs.CmsRecipient class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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


using System.Collections;
using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography.Pkcs {

	public sealed class CmsRecipient {

		private SubjectIdentifierType _recipient;
		private X509Certificate2 _certificate;

		// constructor

		public CmsRecipient (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			_recipient = SubjectIdentifierType.IssuerAndSerialNumber;
			_certificate = certificate;
		}

		public CmsRecipient (SubjectIdentifierType recipientIdentifierType, X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			if (recipientIdentifierType == SubjectIdentifierType.Unknown)
				_recipient = SubjectIdentifierType.IssuerAndSerialNumber;
			else
				_recipient = recipientIdentifierType;
			_certificate = certificate;
		}

		// properties

		public X509Certificate2 Certificate {
			get { return _certificate; }
		}

		public SubjectIdentifierType RecipientIdentifierType {
			get { return _recipient; }
		}
	}
}

