//
// MonoTests.System.Web.Services.Discovery.ContractReferenceTest.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using NUnit.Framework;
using System;
using System.Web.Services.Discovery;

namespace MonoTests.System.Web.Services.Discovery {

	public class ContractReferenceTest : TestCase {

		public ContractReferenceTest () :
			base ("[MonoTests.System.Web.Services.Discovery.ContractReferenceTest]") 
		{
		}

		public ContractReferenceTest (string name) :
			base (name) 
		{
		}

		protected override void SetUp ()
		{
		}

		protected override void TearDown ()
		{
		}

		public static ITest Suite {
			get { return new TestSuite (typeof (ContractReferenceTest)); }
		}

		public void TestConstructors ()
		{
			ContractReference contractReference;

			contractReference = new ContractReference ();
			AssertEquals (String.Empty, contractReference.DocRef);
			AssertEquals (String.Empty, contractReference.Ref);
			AssertEquals (String.Empty, contractReference.Url);
		}

		public void TestConstants ()
		{
			AssertEquals ("http://schemas.xmlsoap.org/disco/scl/", ContractReference.Namespace);
		}
	}
}
