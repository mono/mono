//
// MD5CryptoServiceProviderTest.cs - NUnit Test Cases for MD5 (RFC1321)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using NUnit.Framework;
using Mono.Security.Cryptography;

namespace MonoTests.Security.Cryptography {

	[TestFixture]
	public class MD5CryptoServiceProviderTest : MD5Test {

		[SetUp]
		public void Setup () {
			hash = new MD5CryptoServiceProvider ();
		}

		// this will run ALL tests defined in MD5Test.cs with the MD5CryptoServiceProvider implementation
	}

}