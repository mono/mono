//
// MD4ManagedTest.cs - NUnit Test Cases for MD4 (RFC1320)
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
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
	}
}