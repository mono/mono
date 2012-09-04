//
// System.Net.DefaultCertificatePolicy: Default policy applicable to 
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

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

using System.Security.Cryptography.X509Certificates;

namespace System.Net {

	internal class DefaultCertificatePolicy : ICertificatePolicy {

		// This is the same default policy as used by the .NET 
		// framework. It accepts valid certificates and (valid
		// but) expired certificates.
		public bool CheckValidationResult (ServicePoint point, X509Certificate certificate, WebRequest request, int certificateProblem)
		{
#if SECURITY_DEP
			// If using default policy and the new callback is there, ignore this
			if (ServicePointManager.ServerCertificateValidationCallback != null)
				return true;
#endif
			switch (certificateProblem) {
				case 0:			// No error
				case -2146762495:	// CERT_E_EXPIRED 0x800B0101 (WinError.h)
					return true;
				default:
					return false;
			}
		}
	}
}

