//
// MD2ManagedTest.cs - NUnit Test Cases for MD2 (RFC1319)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

using Mono.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Cryptography {

	[TestFixture]
	public class MD2ManagedTest : MD2Test {

		[SetUp]
		public void Setup () 
		{
			hash = new MD2Managed ();
		}

		// this will run ALL tests defined in MD2Test.cs with the MD2Managed implementation
}

}