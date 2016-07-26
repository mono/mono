//
// X509CertificateInitiatorClientCredential.cs
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
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Security.Tokens; 

namespace System.ServiceModel.Security
{
	public sealed class X509CertificateInitiatorClientCredential
	{
		internal X509CertificateInitiatorClientCredential ()
		{
		}

		X509Certificate2 certificate;

		internal X509CertificateInitiatorClientCredential Clone ()
		{
			return (X509CertificateInitiatorClientCredential) MemberwiseClone ();
		}

		public X509Certificate2 Certificate {
			get { return certificate; }
			set { certificate = value; }
		}

		public void SetCertificate (StoreLocation storeLocation,
			StoreName storeName, X509FindType findType,
			object findValue)
		{
#if !NET_2_1
			certificate = ConfigUtil.CreateCertificateFrom (storeLocation, storeName, findType, findValue);
#else
			throw new NotImplementedException ();
#endif
		}

		public void SetCertificate (
			string subjectName, StoreLocation storeLocation,
			StoreName storeName)
		{
#if !NET_2_1
			certificate = ConfigUtil.CreateCertificateFrom (storeLocation, storeName, X509FindType.FindBySubjectName, subjectName);
#else
			throw new NotImplementedException ();
#endif
		}
	}
}
