//
// MD4CryptoServiceProviderTest.cs - NUnit Test Cases for MD4 (RFC1320)
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
public class MD4CryptoServiceProviderTest : MD4Test {

	[SetUp]
	public void Setup () 
	{
		hash = new MD4CryptoServiceProvider ();
	}

	// this will run ALL tests defined in MD4Test.cs with the MD4CryptoServiceProvider implementation
}

}