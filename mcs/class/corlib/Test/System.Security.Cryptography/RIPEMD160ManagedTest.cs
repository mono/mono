//
// RIPEMD160ManagedTest.cs - NUnit Test Cases for RIPEMD160
//	http://www.esat.kuleuven.ac.be/~bosselae/ripemd160.html
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using NUnit.Framework;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class RIPEMD160ManagedTest : RIPEMD160Test {

		[SetUp]
		public void Setup () 
		{
			hash = new RIPEMD160Managed ();
		}

		// this will run ALL tests defined in RIPEMD160Test.cs with the RIPEMD160Managed implementation
	}
}

#endif