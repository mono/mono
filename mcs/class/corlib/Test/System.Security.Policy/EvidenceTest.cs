//
// MonoTests.System.Security.Policy.EvidenceTest
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Jackson Harper, All rights reserved.
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		public void Constructor_Null ()
		{
			Evidence e = new Evidence (null);
			AssertEquals ("Count-Empty", 0, e.Count);
		}

		[Test]
		public void Constructor_NullNull ()
		{
			Evidence e = new Evidence (null, null);
			AssertEquals ("Count-Empty", 0, e.Count);
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
			Evidence evidence = new Evidence ();
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
			AssertEquals ("Count", evidence.Count, evidence2.Count);
		}

		[Test]
		public void Merge ()
		{
			Evidence evidence = new Evidence (new object[10], new object[10]);
		  	Evidence evidence2 = new Evidence ();
			evidence2.Merge (evidence);
			AssertEquals ("Count", evidence.Count, evidence2.Count);
		}

		[Test]
		public void Merge_Null ()
		{
			Evidence evidence = new Evidence ();
			evidence.Merge (null);
			// no exception!
			AssertEquals ("Count", 0, evidence.Count);
		}

		[Test]
		public void DefaultProperties () 
		{
			Evidence e = new Evidence ();
			AssertEquals ("Count", 0, e.Count);
			Assert ("IsReadOnly", !e.IsReadOnly);
#if NET_2_0
			Assert ("IsSynchronized", !e.IsSynchronized);
#else
			// LAMESPEC: Always TRUE (not FALSE)
			Assert ("IsSynchronized", e.IsSynchronized);
#endif
			Assert ("Locked", !e.Locked);
			AssertNotNull ("SyncRoot", e.SyncRoot);
		}

#if NET_2_0
		[Test]
		public void Equals_GetHashCode () 
		{
			Evidence e1 = new Evidence ();
			Evidence e2 = new Evidence ();
			AssertEquals ("GetHashCode-1", e1.GetHashCode (), e2.GetHashCode ());
			Assert ("e1.Equals(e2)", e1.Equals (e2));
			e1.AddAssembly (String.Empty);
			e2.AddAssembly (String.Empty);
			AssertEquals ("GetHashCode-2", e1.GetHashCode (), e2.GetHashCode ());
			e1.AddHost (String.Empty);
			e2.AddHost (String.Empty);
			AssertEquals ("GetHashCode-3", e1.GetHashCode (), e2.GetHashCode ());
			Assert ("e2.Equals(e1)", e2.Equals (e1));
		}

		[Test]
		public void Clear () 
		{
			Evidence e = new Evidence ();
			AssertEquals ("Count-Empty", 0, e.Count);
			e.AddAssembly (new object ());
			AssertEquals ("Count+Assembly", 1, e.Count);
			e.AddHost (new object ());
			AssertEquals ("Count+Host", 2, e.Count);
			e.Clear ();
			AssertEquals ("Count-Cleared", 0, e.Count);
		}

		[Test]
		public void RemoveType ()
		{
			Evidence e = new Evidence ();
			AssertEquals ("Count-Empty", 0, e.Count);
			e.AddAssembly (new object ());
			e.AddHost (new object ());
			AssertEquals ("Count", 2, e.Count);
			e.RemoveType (typeof (object));
			AssertEquals ("Count-RemoveType(object)", 0, e.Count);
		}
#else
		[Test]
		public void Equals_GetHashCode () 
		{
			Evidence e1 = new Evidence ();
			Evidence e2 = new Evidence ();
			Assert ("GetHashCode", e1.GetHashCode () != e2.GetHashCode ());
			Assert ("!e1.Equals(e2)", !e1.Equals (e2));
			Assert ("!e2.Equals(e1)", !e2.Equals (e1));
		}
#endif
	}
}
