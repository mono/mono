//
// X509ServiceCertificateAuthentication.cs
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
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.IdentityModel.Selectors;

namespace System.ServiceModel.Security
{
	[MonoTODO]
	public class X509ServiceCertificateAuthentication
	{
		public X509ServiceCertificateAuthentication ()
		{
		}

		X509CertificateValidationMode validation_mode;
		X509CertificateValidator custom_validator;
		X509RevocationMode revocation_mode;
		StoreLocation trusted_store_loc;

		internal X509ServiceCertificateAuthentication Clone ()
		{
			return (X509ServiceCertificateAuthentication) MemberwiseClone ();
		}

		public X509CertificateValidationMode CertificateValidationMode {
			get { return validation_mode; }
			set { validation_mode = value; }
		}

		public X509CertificateValidator CustomCertificateValidator {
			get { return custom_validator; }
			set { custom_validator = value; }
		}

		public X509RevocationMode RevocationMode {
			get { return revocation_mode; }
			set { revocation_mode = value; }
		}

		public StoreLocation TrustedStoreLocation { 
			get { return trusted_store_loc; }
			set { trusted_store_loc = value; }
		}
	}
}
