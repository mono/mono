//
// X509ChainTest.cs - NUnit tests for X509Chain
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Security.Cryptography.X509Certificates;

namespace MonoTests.System.Security.Cryptography.X509Certificates {

	[TestFixture]
	public class X509ChainTest : Assertion {

		[Test]
		public void ConstructorEmpty () 
		{
			X509Chain c = new X509Chain ();
			// default properties
			AssertEquals ("ChainElements", 0, c.ChainElements.Count);
			AssertNotNull ("ChainPolicy", c.ChainPolicy);
			AssertEquals ("ChainStatus", 0, c.ChainStatus.Length);
		}

		[Test]
		public void ConstructorMachineContextFalse () 
		{
			X509Chain c = new X509Chain (false);
			// default properties
			AssertEquals ("ChainElements", 0, c.ChainElements.Count);
			AssertNotNull ("ChainPolicy", c.ChainPolicy);
			AssertEquals ("ChainStatus", 0, c.ChainStatus.Length);
		}

		[Test]
		public void ConstructorMachineContextTrue () 
		{
			X509Chain c = new X509Chain (true);
			// default properties
			AssertEquals ("ChainElements", 0, c.ChainElements.Count);
			AssertNotNull ("ChainPolicy", c.ChainPolicy);
			AssertEquals ("ChainStatus", 0, c.ChainStatus.Length);
		}

		[Test]
		public void StaticCreation () 
		{
			X509Chain c = X509Chain.Create ();
			// default properties
			AssertEquals ("ChainElements", 0, c.ChainElements.Count);
			AssertNotNull ("ChainPolicy", c.ChainPolicy);
			AssertEquals ("ChainStatus", 0, c.ChainStatus.Length);
		}
	}
}

#endif
