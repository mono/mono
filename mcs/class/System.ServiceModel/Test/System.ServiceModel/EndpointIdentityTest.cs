//
// EndpointIdentityTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
#if !MOBILE
using System;
using System.IO;
using System.IdentityModel.Claims;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.ServiceModel;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class EndpointIdentityTest
	{
		static readonly X509Certificate2 cert = new X509Certificate2 (TestResourceHelper.GetFullPathOfResource ("Test/Resources/test.cer"));

		[Test]
		public void CreateX509CertificateIdentity ()
		{
			X509CertificateEndpointIdentity identity =
				EndpointIdentity.CreateX509CertificateIdentity (cert)
				as X509CertificateEndpointIdentity;
			Claim c = identity.IdentityClaim;
			Assert.IsNotNull (c, "#1");
			Assert.AreEqual (ClaimTypes.Thumbprint, c.ClaimType, "#2");
			DataContractSerializer ser = new DataContractSerializer (c.GetType ());
			StringWriter sw = new StringWriter ();
			XmlWriter xw = XmlWriter.Create (sw);
			ser.WriteObject (xw, c);
			xw.Close ();
			string xml = @"<?xml version=""1.0"" encoding=""utf-16""?><Claim xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://schemas.xmlsoap.org/ws/2005/05/identity""><ClaimType>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint</ClaimType><Resource xmlns:d2p1=""http://www.w3.org/2001/XMLSchema"" i:type=""d2p1:base64Binary"">GQ3YHlGQhDF1bvMixHliX4uLjlY=</Resource><Right>http://schemas.xmlsoap.org/ws/2005/05/identity/right/possessproperty</Right></Claim>";
			Assert.AreEqual (C14N (xml), C14N (sw.ToString ()), "#3");
			Assert.AreEqual ("identity(" + c + ")", identity.ToString (), "#4");
		}

		string C14N (string xml)
		{
			XmlDsigExcC14NTransform t = new XmlDsigExcC14NTransform ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			t.LoadInput (doc);
			return new StreamReader (t.GetOutput () as Stream).ReadToEnd ();
		}
	}
}
#endif

