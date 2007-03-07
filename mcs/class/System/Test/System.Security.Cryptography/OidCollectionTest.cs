//
// OidCollectionTest.cs - NUnit tests for OidCollection
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_2_0

using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
#if TARGET_JVM
	[Ignore ("The class System.Security.Cryptography.OidCollection - is not supported")]
#endif
	public class OidCollectionTest : Assertion {
#if !TARGET_JVM

		[Test]
		public void Constructor () 
		{
			OidCollection oc = new OidCollection ();
			// default properties
			AssertEquals ("Count", 0, oc.Count);
			Assert ("IsSynchronized", !oc.IsSynchronized);
			AssertNotNull ("SyncRoot", oc.SyncRoot);
			AssertNotNull ("GetEnumerator", oc.GetEnumerator ());
		}

		[Test]
		public void Add ()
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.0"));
			AssertEquals ("Count", 1, oc.Count);
			AssertEquals ("[0]", "1.0", oc [0].Value);
			AssertEquals ("['1.0']", "1.0", oc ["1.0"].Value);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void AddNull () 
		{
			OidCollection oc = new OidCollection ();
			oc.Add (null);
			AssertEquals ("Count", 1, oc.Count);
			// AssertNull ("[0]", oc); throw NullReferenceException
		}

		[Test]
		public void CopyToOid () 
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.0"));
			Oid[] array = new Oid [1];
			oc.CopyTo (array, 0);
			AssertEquals ("CopyTo(Oid)", "1.0", array [0].Value);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyToOidNull ()
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.0"));
			Oid[] array = null;
			oc.CopyTo (array, 0);
		}
#endif
	}
}

#endif
