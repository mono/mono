//
// System.Net.DefaultCertificatePolicy: Default policy applicable to 
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System.Security.Cryptography.X509Certificates;

namespace System.Net {

	internal class DefaultCertificatePolicy : ICertificatePolicy {

		// This is the same default policy as used by the .NET 
		// framework. It accepts valid certificates and (valid
		// but) expired certificates.
		public bool CheckValidationResult (ServicePoint point, X509Certificate certificate, WebRequest request, int certificateProblem)
		{
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
