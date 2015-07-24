//
// OidEnumeratorTest.cs - NUnit tests for OidEnumerator
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//


using NUnit.Framework;

using System;
using System.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class OidEnumeratorTest {
		private OidEnumerator GetEnumerator () 
		{
			OidCollection oc = new OidCollection ();
			oc.Add (new Oid ("1.0"));
			oc.Add (new Oid ("1.1"));
			oc.Add (new Oid ("1.2"));
			return oc.GetEnumerator ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Current_BeforeFirstElement ()
		{
			OidEnumerator enumerator = GetEnumerator ();
			Oid oid = enumerator.Current;
		}

		[Test]
		public void Current () 
		{
			OidEnumerator enumerator = GetEnumerator ();
			enumerator.MoveNext ();
			Oid oid = enumerator.Current;
			Assert.IsNotNull (oid, "Current");
		}

		[Test]
		public void Current_AfterLastElement ()
		{
			OidEnumerator enumerator = GetEnumerator ();
			while (enumerator.MoveNext ());
			Oid oid = enumerator.Current;
			Assert.IsNotNull (oid, "Current_AfterLastElement");
			Assert.AreEqual ("1.2", oid.Value, "Current==last");
		}

		[Test]
		public void MoveNext () 
		{
			OidEnumerator enumerator = GetEnumerator ();
			int n = 0;
			while (enumerator.MoveNext ()) {
				n++;
			}
			Assert.AreEqual (3, n, "MoveNext");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Reset () 
		{
			OidEnumerator enumerator = GetEnumerator ();
			enumerator.MoveNext ();
			Assert.IsNotNull (enumerator.Current, "Current before reset");
			enumerator.Reset ();
			Assert.IsNotNull (enumerator.Current, "Current after reset");
		}
	}
}

