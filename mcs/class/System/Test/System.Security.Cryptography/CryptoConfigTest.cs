//
// CryptoConfig.cs - NUnit tests for CryptoConfig
//
// Author:
//	Chris Hamons <chris.hamons@microsoft.com>
//
// Copyright (c) 2017 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.



using NUnit.Framework;

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography
{

	[TestFixture]
	public class CryptoConfigTest
	{
		[Test]
		[TestCase ("http://www.w3.org/2000/09/xmldsig#dsa-sha1")]
		[TestCase ("http://www.w3.org/2000/09/xmldsig#rsa-sha1")]
		[TestCase ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")]
		[TestCase ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha384")]
		[TestCase ("http://www.w3.org/2001/04/xmldsig-more#rsa-sha512")]
		public void CryptoConfig_NonNullDigest (string name)
		{
			SignatureDescription signatureDescription = CryptoConfig.CreateFromName (name) as SignatureDescription;
			Assert.NotNull (signatureDescription.CreateDigest ());
		}
	}
}

