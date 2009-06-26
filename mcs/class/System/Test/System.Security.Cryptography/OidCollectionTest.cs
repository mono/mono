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
	public class OidCollectionTest {
#if !TARGET_JVM

		[Test]
		public void Constructor () 
		{
			OidCollection oc = new OidCollection ();
			// default properties
			Assert.AreEqual (0, oc.Count, "Count");
			Assert.IsTrue (!oc.IsSynchronized, "IsSynchronized");
			Assert.IsNotNull (oc.SyncRoot, "SyncRoot");
			Assert.IsNotNull (oc.GetEnumerator (), "GetEnumerator");
		}

		[Test]
		public void Add ()
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.0"));
			Assert.AreEqual (1, oc.Count, "Count");
			Assert.AreEqual ("1.0", oc [0].Value, "[0]");
			Assert.AreEqual ("1.0", oc ["1.0"].Value, "['1.0']");
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void AddNull () 
		{
			OidCollection oc = new OidCollection ();
			oc.Add (null);
			Assert.AreEqual (1, oc.Count, "Count");
			// Assert.IsNull (oc, "[0]"); throw NullReferenceException
		}

		[Test]
		public void CopyToOid () 
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.0"));
			Oid[] array = new Oid [1];
			oc.CopyTo (array, 0);
			Assert.AreEqual ("1.0", array [0].Value, "CopyTo(Oid)");
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
