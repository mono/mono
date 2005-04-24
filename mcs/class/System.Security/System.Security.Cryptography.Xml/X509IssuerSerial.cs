//
// X509IssuerSerial.cs - X509IssuerSerial implementation for XML Encryption
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
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

namespace System.Security.Cryptography.Xml {

#if NET_2_0
	public
#else
	// structure was undocumented (but present) before Fx 2.0
	internal
#endif
	struct X509IssuerSerial {

		private string _issuerName;
		private string _serialNumber;

		internal X509IssuerSerial (string issuer, string serial) 
		{
			_issuerName = issuer;
			_serialNumber = serial;
		}

		public string IssuerName {
			get { return _issuerName; }
			set { _issuerName = value; }
		}

		public string SerialNumber {
			get { return _serialNumber; }
			set { _serialNumber = value; }
		}
	}
}

