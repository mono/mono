//
// MonoTests.System.Security.Policy.EvidenceTest
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2001 Jackson Harper, All rights reserved.
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Policy;
using NUnit.Framework;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class EvidenceTest : Assertion {
		
		[Test]
		public void DefaultConstructor ()
		{
			Evidence evidence = new Evidence ();
			
			AssertEquals ("Default constructor count should be zero", evidence.Count, 0);
			AssertEquals ("Default constructor host enumerator MoveNext() should be false", 
				evidence.GetHostEnumerator().MoveNext(), false);
			AssertEquals ("Default constructor assembly enumerator MoveNext() should be false",
				evidence.GetAssemblyEnumerator().MoveNext(), false);
			AssertEquals ("Default constructor enumerator MoveNext() should be false",
				evidence.GetEnumerator().MoveNext(), false);
		}

		[Test]
		public void MultipleConstructor ()
		{
			object[] hostarray = new object[10];
			object[] assemarray = new object[10];
			Evidence evidence = new Evidence ( hostarray, assemarray );

			AssertEquals ( "Count of multiple arg constructor should equal 20", evidence.Count, 20 );
		}

		[Test]
		public void CopyConstructor ()
		{
			object[] hostlist = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmblist = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			Evidence evidence1 = new Evidence (hostlist, asmblist);
			Evidence evidence2 = new Evidence (evidence1);
			
			AssertEquals("Copy constructor counts do not match", evidence1.Count, evidence2.Count);
		}

		[Test]
		public void AddAssembly ()
		{
			Evidence evidence = new Evidence ();
			object[] comparray = new object[100];
			string obj;

			for (int i=0; i<100; i++) {
				obj = String.Format ("asmb-{0}", i+1);
				comparray[i] = obj;
				evidence.AddAssembly (obj);
				AssertEquals (evidence.Count, i+1);
			}
			
			int index = 0;
			foreach (object compobj in evidence) {
				AssertEquals ("Comparison object does not equal evidence assembly object", 
					comparray[index++], compobj);
			}
		}

		[Test]
		public void AddHost ()
		{
			Evidence evidence = new Evidence ();
			object[] comparray = new object[100];
			string obj;

			for (int i=0; i<100; i++) {
				obj = String.Format ("asmb-{0}", i+1);
				comparray[i] = obj;
				evidence.AddAssembly ( obj );
				AssertEquals (evidence.Count, i+1);
			}

			int index = 0;
			foreach (object compobj in evidence) {
				AssertEquals ("Comparison object does not equal evidence host object", 
					comparray[index++], compobj);
			}
		}

		[Test]
		public void MultiArgConstructorForEach ()
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			ArrayList compare = new ArrayList (); 
			Evidence evidence = new Evidence (hostarray, asmbarray);
		
			compare.AddRange (hostarray);
			compare.AddRange (asmbarray);
	
			int i = 0;		
			foreach (object obj in evidence) {
				AssertEquals (obj, compare[i++]);
			}
		}

		[Test]
		public void EnumeratorReset ()
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			ArrayList compare = new ArrayList (); 
			Evidence evidence = new Evidence (hostarray, asmbarray);
			compare.AddRange (hostarray);
			compare.AddRange (asmbarray);

			int i = 0;
			IEnumerator enumerator = evidence.GetEnumerator ();	
			while (enumerator.MoveNext ()) {
				AssertEquals (enumerator.Current, compare[i++]);
			}

			enumerator.Reset ();
			i = 0;
			while (enumerator.MoveNext ()) {
				AssertEquals (enumerator.Current, compare[i++]);
			}
		}

		[Test]
		public void GetHostEnumerator ()
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2" };
			Evidence evidence = new Evidence (hostarray, asmbarray);
			IEnumerator enumerator = evidence.GetHostEnumerator ();
			int i = 0;
			while (enumerator.MoveNext ()) {
               			AssertEquals (enumerator.Current, hostarray[i++]);
			}
		}

		[Test]
		public void GetHostAssemblyEnumerator ()
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			Evidence evidence;
			IEnumerator enumerator;
			int i;

			evidence = new Evidence (hostarray, asmbarray);
			enumerator = evidence.GetAssemblyEnumerator ();
			
			i = 0;
			while (enumerator.MoveNext()) {
        	        	AssertEquals (enumerator.Current, asmbarray[i++]);
			}
		}

		[Test]
		public void Count ()
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			Evidence evidence = new Evidence (hostarray, asmbarray);
			Assertion.AssertEquals (evidence.Count, 8);

			for( int i=0; i<100; i++ ) {
				if ( 0 == i%2 ) {
					evidence.AddHost (String.Format ("host-{0}", i + 5) );
				} else {
					evidence.AddAssembly (String.Format ("asmb-{0}", i + 5));
				}
				AssertEquals (evidence.Count, 9 + i);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullCopyToException() 
		{
			Evidence evidence = new Evidence ();
			evidence.AddHost ("host-1");
			evidence.CopyTo (null, 100);
		}

		/// <summary>
		///    No Exception will be generated because the copy won't run because the evidence list is empty
		/// </summary>
		[Test]
		public void CopyToNoException() 
		{
			Evidence evidence = new Evidence ();;
			evidence.CopyTo (null, 100);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ArgOutOfRangeCopyToException() 
		{
			Evidence evidence = new Evidence (new object[10], new object[10]);
			evidence.CopyTo (new object[10], -100);
		}

		/// <summary>
		///    No Exception will be generated because the copy won't run because the evidence list is empty
		/// </summary>
		[Test]
		public void ArgOutOfRangeCopyToNoException() 
		{
			Evidence evidence = new Evidence ();
			evidence.CopyTo (new object[10], -100);
		}

		[Test]
		public void BadMerge ()
		{
			Evidence evidence = new Evidence (null, null);
			Evidence evidence2 = new Evidence ();
			evidence2.Merge (evidence);
		}

		[Test]
		public void Merge ()
		{
			Evidence evidence = new Evidence (new object[10], new object[10]);
		  	Evidence evidence2 = new Evidence ();
			evidence2.Merge (evidence);
		}

		[Test]
		public void DefaultProperties () 
		{
			Evidence e = new Evidence ();
			AssertEquals ("Count", 0, e.Count);
			Assert ("IsReadOnly", !e.IsReadOnly);
			// LAMESPEC: Always TRUE (not FALSE)
			Assert ("IsSynchronized", e.IsSynchronized);
			Assert ("Locked", !e.Locked);
			AssertNotNull ("SyncRoot", e.SyncRoot);
		}
	}
}
