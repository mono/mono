//
// X509PeerCertificateAuthentication.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Selectors;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	[MonoTODO]
	public class X509PeerCertificateAuthentication
	{
		internal X509PeerCertificateAuthentication ()
		{
		}

		X509CertificateValidator validator;
		X509RevocationMode revocation_mode = X509RevocationMode.Online;
		StoreLocation store_loc = StoreLocation.CurrentUser;
		X509CertificateValidationMode validation_mode =
			X509CertificateValidationMode.PeerOrChainTrust;

		internal X509PeerCertificateAuthentication Clone ()
		{
			return (X509PeerCertificateAuthentication) MemberwiseClone ();
		}

		public X509CertificateValidator CustomCertificateValidator {
			get { return validator; }
			set { validator = value; }
		}

		public X509RevocationMode RevocationMode {
			get { return revocation_mode; }
			set { revocation_mode = value; }
		}

		public StoreLocation TrustedStoreLocation {
			get { return store_loc; }
			set { store_loc = value; }
		}

		public X509CertificateValidationMode CertificateValidationMode {
			get { return validation_mode; }
			set { validation_mode = value; }
		}
	}
}
