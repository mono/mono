//
// IssuedTokenServiceCredential.cs
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
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	[MonoTODO]
	public class IssuedTokenServiceCredential
	{
		bool allow_untrusted_rsa_issuers;
		X509CertificateValidationMode cert_verify_mode =
			X509CertificateValidationMode.ChainTrust;
		X509CertificateValidator custom_cert_validator;
		List<X509Certificate2> known_certs = new List<X509Certificate2> ();
		X509RevocationMode revocation_mode = X509RevocationMode.Online;
		SamlSerializer saml_serializer;
		StoreLocation store_location = StoreLocation.LocalMachine;

		internal IssuedTokenServiceCredential ()
		{
			AllowedAudienceUris = new List<string> ();
		}

		internal IssuedTokenServiceCredential Clone ()
		{
			var ret = (IssuedTokenServiceCredential) MemberwiseClone ();
			ret.known_certs = new List<X509Certificate2> (known_certs);
			return ret;
		}

		[MonoTODO]
		public IList<string> AllowedAudienceUris { get; private set; }

		[MonoTODO]
		public bool AllowUntrustedRsaIssuers {
			get { return allow_untrusted_rsa_issuers; }
			set { allow_untrusted_rsa_issuers = value; }
		}

		[MonoTODO]
		public AudienceUriMode AudienceUriMode { get; set; }

		[MonoTODO]
		public X509CertificateValidationMode CertificateValidationMode {
			get { return cert_verify_mode; }
			set { cert_verify_mode = value; }
		}

		[MonoTODO]
		public X509CertificateValidator CustomCertificateValidator {
			get { return custom_cert_validator; }
			set { custom_cert_validator = value; }
		}

		[MonoTODO]
		public IList<X509Certificate2> KnownCertificates {
			get { return known_certs; }
		}

		[MonoTODO]
		public X509RevocationMode RevocationMode {
			get { return revocation_mode; }
			set { revocation_mode = value; }
		}

		[MonoTODO]
		public SamlSerializer SamlSerializer {
			get { return saml_serializer; }
			set { saml_serializer = value; }
		}

		[MonoTODO]
		public StoreLocation TrustedStoreLocation {
			get { return store_location; }
			set { store_location = value; }
		}
	}
}
