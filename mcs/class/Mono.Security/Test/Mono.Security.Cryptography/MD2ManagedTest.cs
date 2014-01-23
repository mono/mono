//
// MD2ManagedTest.cs - NUnit Test Cases for MD2 (RFC1319)
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;

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
		
		[Test]
		public override void Create () 
		{
			// try creating ourselve using Create
			HashAlgorithm h = MD2.Create ("MD2Managed");
			Assert.IsTrue ((h is MD2Managed), "MD2Managed");
		}
	}
}
