//
// MD4CryptoServiceProviderTest.cs - NUnit Test Cases for MD2 (RFC1319)
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
public class MD2CryptoServiceProviderTest : MD2Test {

	[SetUp]
	public void Setup () 
	{
		hash = new MD2CryptoServiceProvider ();
	}

	// this will run ALL tests defined in MD2Test.cs with the MD2CryptoServiceProvider implementation
}

}