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
using System.Reflection;
using System.Security.Policy;
using NUnit.Framework;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class EvidenceTest  {
		
		[Test]
		public void DefaultConstructor ()
		{
			Evidence evidence = new Evidence ();
			
			Assert.AreEqual (evidence.Count, 0, "Default constructor count should be zero");
			Assert.AreEqual (evidence.GetHostEnumerator().MoveNext(), false, 
				"Default constructor host enumerator MoveNext() should be false");
				
			Assert.AreEqual (evidence.GetAssemblyEnumerator().MoveNext(), false, 
						  "Default constructor assembly enumerator MoveNext() should be false");
				
			Assert.AreEqual (evidence.GetEnumerator().MoveNext(), false,
						  "Default constructor enumerator MoveNext() should be false");
		}

		[Test]
		public void MultipleConstructor ()
		{
			object[] hostarray = new object[10];
			object[] assemarray = new object[10];
			Evidence evidence = new Evidence ( hostarray, assemarray );

			Assert.AreEqual (evidence.Count, 20,
						  "Count of multiple arg constructor should equal 20");
		}

		[Test]
		public void CopyConstructor ()
		{
			object[] hostlist = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmblist = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			Evidence evidence1 = new Evidence (hostlist, asmblist);
			Evidence evidence2 = new Evidence (evidence1);
			
			Assert.AreEqual(evidence1.Count, evidence2.Count, "Copy constructor counts do not match");
		}

		[Test]
		public void Constructor_Null ()
		{
			Evidence e = new Evidence (null);
			Assert.AreEqual (0, e.Count, "Count-Empty");
		}

		[Test]
		public void Constructor_NullNull ()
		{
			Evidence e = new Evidence (null, null);
			Assert.AreEqual (0, e.Count, "Count-Empty");
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
				Assert.AreEqual (evidence.Count, i+1);
			}
			
			int index = 0;
			foreach (object compobj in evidence) {
				Assert.AreEqual (comparray[index++], compobj, "Comparison object does not equal evidence assembly object");
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
				Assert.AreEqual (evidence.Count, i+1);
			}

			int index = 0;
			foreach (object compobj in evidence) {
				Assert.AreEqual (comparray[index++], compobj, "Comparison object does not equal evidence host object");
					
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
				Assert.AreEqual (obj, compare[i++]);
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
				Assert.AreEqual (enumerator.Current, compare[i++]);
			}

			enumerator.Reset ();
			i = 0;
			while (enumerator.MoveNext ()) {
				Assert.AreEqual (enumerator.Current, compare[i++]);
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
               			Assert.AreEqual (enumerator.Current, hostarray[i++]);
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
        	        	Assert.AreEqual (enumerator.Current, asmbarray[i++]);
			}
		}

		[Test]
		public void Count ()
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			Evidence evidence = new Evidence (hostarray, asmbarray);
			Assert.AreEqual (evidence.Count, 8);

			for( int i=0; i<100; i++ ) {
				if ( 0 == i%2 ) {
					evidence.AddHost (String.Format ("host-{0}", i + 5) );
				} else {
					evidence.AddAssembly (String.Format ("asmb-{0}", i + 5));
				}
				Assert.AreEqual (evidence.Count, 9 + i);
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
			Assert.AreEqual (evidence.Count, evidence2.Count, "Count");
		}

		[Test]
		public void Merge ()
		{
			Evidence evidence = new Evidence (new object[10], new object[10]);
		  	Evidence evidence2 = new Evidence ();
			evidence2.Merge (evidence);
			Assert.AreEqual (evidence.Count, evidence2.Count, "Count");
		}

		[Test]
		public void Merge_Null ()
		{
			Evidence evidence = new Evidence ();
			evidence.Merge (null);
			// no exception!
			Assert.AreEqual (0, evidence.Count, "Count");
		}

		[Test]
		public void DefaultProperties () 
		{
			Evidence e = new Evidence ();
			Assert.AreEqual (0, e.Count, "Count");
			Assert.IsTrue (!e.IsReadOnly, "IsReadOnly");
			Assert.IsTrue (!e.IsSynchronized, "IsSynchronized");
			Assert.IsTrue (!e.Locked, "Locked");
			Assert.IsNotNull (e.SyncRoot, "SyncRoot");
		}

#if !NET_4_0
		[Test]
		public void Equals_GetHashCode () 
		{
			Evidence e1 = new Evidence ();
			Evidence e2 = new Evidence ();
			Assert.AreEqual (e1.GetHashCode (), e2.GetHashCode (), "GetHashCode-1");
			Assert.IsTrue (e1.Equals (e2), "e1.Equals(e2)");
			e1.AddAssembly (String.Empty);
			e2.AddAssembly (String.Empty);
			Assert.AreEqual (e1.GetHashCode (), e2.GetHashCode (), "GetHashCode-2");
			e1.AddHost (String.Empty);
			e2.AddHost (String.Empty);
			Assert.AreEqual (e1.GetHashCode (), e2.GetHashCode (), "GetHashCode-3");
			Assert.IsTrue (e2.Equals (e1), "e2.Equals(e1)");
		}
#endif

		[Test]
		public void Clear () 
		{
			Evidence e = new Evidence ();
			Assert.AreEqual (0, e.Count, "Count-Empty");
			e.AddAssembly (new object ());
			Assert.AreEqual (1, e.Count, "Count+Assembly");
			e.AddHost (new object ());
			Assert.AreEqual (2, e.Count, "Count+Host");
			e.Clear ();
			Assert.AreEqual (0, e.Count, "Count-Cleared");
		}

		[Category ("NotWorking")]
		[Test]
		public void RemoveType ()
		{
			Evidence e = new Evidence ();
			Assert.AreEqual (0, e.Count, "Count-Empty");
			e.AddAssembly (new object ());
			e.AddHost (new object ());
			Assert.AreEqual (2, e.Count, "Count");
			e.RemoveType (typeof (object));
			Assert.AreEqual (0, e.Count, "Count-RemoveType(object)");
		}

		[Test]
		public void AppDomain_NoPermissionRequestEvidence ()
		{
			// PermissionRequestEvidence is only used druing policy resolution
			// and can't be accessed using the Evidence property
			Evidence e = AppDomain.CurrentDomain.Evidence;
			foreach (object o in e) {
				if (o is PermissionRequestEvidence)
					Assert.Fail ("Found PermissionRequestEvidence in AppDomain.CurrentDomain.Evidence");
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Assembly_NoPermissionRequestEvidence ()
		{
			// PermissionRequestEvidence is only used druing policy resolution
			// and can't be accessed using the Evidence property
			Evidence e = Assembly.GetExecutingAssembly ().Evidence;
			foreach (object o in e) {
				if (o is PermissionRequestEvidence)
					Assert.Fail ("Found PermissionRequestEvidence in Assembly.GetExecutingAssembly.Evidence");
			}
		}
	}
}
