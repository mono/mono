//
// MD4ManagedTest.cs - NUnit Test Cases for MD4 (RFC1320)
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;
using Mono.Security.Cryptography;

namespace MonoTests.Mono.Security.Cryptography {

	[TestFixture]
	public class MD4ManagedTest : MD4Test {

		[SetUp]
		public void Setup () 
		{
			hash = new MD4Managed ();
		}

		// this will run ALL tests defined in MD4Test.cs with the MD4Managed implementation
		
		[Test]
		public override void Create () 
		{
			// try creating ourselve using Create
			HashAlgorithm h = MD4.Create ("MD4Managed");
			Assert.IsTrue ((h is MD4Managed), "MD4Managed");
		}
	}
}
