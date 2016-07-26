//
// X509CertificateRecipientClientCredential.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
#if !NET_2_1
using System.IdentityModel.Selectors;
#endif
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Security
{
	public sealed class X509CertificateRecipientClientCredential
	{
		internal X509CertificateRecipientClientCredential ()
		{
		}

		X509ServiceCertificateAuthentication auth =
			new X509ServiceCertificateAuthentication ();
		X509Certificate2 certificate;
		Dictionary<Uri,X509Certificate2> scoped =
			new Dictionary<Uri,X509Certificate2> ();
#if !NET_2_1
		X509CertificateValidator validator;
#endif
		X509RevocationMode revocation_mode;
		StoreLocation store_loc;

		internal X509CertificateRecipientClientCredential Clone ()
		{
			var ret = (X509CertificateRecipientClientCredential) MemberwiseClone ();
			ret.auth = auth.Clone ();
			ret.scoped = new Dictionary<Uri,X509Certificate2> (scoped);
			return ret;
		}

		public X509ServiceCertificateAuthentication Authentication {
			get { return auth; }
		}

		public X509Certificate2 DefaultCertificate {
			get { return certificate; }
			set { certificate = value; }
		}

		public Dictionary<Uri,X509Certificate2> ScopedCertificates {
			get { return scoped; }
		}

		[MonoTODO]
		public X509ServiceCertificateAuthentication SslCertificateAuthentication
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public void SetDefaultCertificate (string subjectName,
			StoreLocation storeLocation, StoreName storeName)
		{
			SetDefaultCertificate (storeLocation, storeName, X509FindType.FindBySubjectName, subjectName);
		}

		public void SetDefaultCertificate (StoreLocation storeLocation,
			StoreName storeName, X509FindType findType, Object findValue)
		{
#if !NET_2_1
			DefaultCertificate = ConfigUtil.CreateCertificateFrom (storeLocation, storeName, findType, findValue);
#else
			throw new NotImplementedException ();
#endif
		}

		public void SetScopedCertificate (string subjectName,
			StoreLocation storeLocation, StoreName storeName,
			Uri targetService)
		{
			SetScopedCertificate (storeLocation, storeName, X509FindType.FindBySubjectName, subjectName, targetService);
		}

		public void SetScopedCertificate (StoreLocation storeLocation,
			StoreName storeName, X509FindType findType,
			Object findValue, Uri targetService)
		{
#if !NET_2_1
			ScopedCertificates [targetService] = ConfigUtil.CreateCertificateFrom (storeLocation, storeName, findType, findValue);
#else
			throw new NotImplementedException ();
#endif
		}
	}
}
