//
// MonoTests.System.Security.Policy.EvidenceTest
//
// Author(s):
//   Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2001 Jackson Harper, All rights reserved.


using System;
using System.Collections;
using System.Security.Policy;
using NUnit.Framework;


namespace MonoTests.System.Security.Policy
{

	public class EvidenceTest : TestCase 
	{
		
		public EvidenceTest(string name): base(name)
		{
		}
		
		public EvidenceTest() : base("EvidenceTest")
		{
		}

		public static ITest Suite
		{
			get {
				return new TestSuite(typeof(EvidenceTest));
			}
		}

		protected override void SetUp()
		{
		}

		public void TestDefaultConstructor()
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

		public void TestMultipleConstructor() {
			Evidence evidence;
			object[] hostarray = new object[10];
			object[] assemarray = new object[10];

			evidence = new Evidence ( hostarray, assemarray );

			AssertEquals ( "Count of multiple arg constructor should equal 20", evidence.Count, 20 );
		}

		public void TestCopyConstructor() 
		{
			Evidence evidence1, evidence2;
			object[] hostlist = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmblist = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };

			evidence1 = new Evidence (hostlist, asmblist);
			evidence2 = new Evidence (evidence1);
			
			AssertEquals("Copy constructor counts do not match", evidence1.Count, evidence2.Count);
		}

		public void TestAddAssembly() 
		{
			Evidence evidence = new Evidence ();
			object[] comparray = new object[100];
			string obj;
			int index;

			for (int i=0; i<100; i++) {
				obj = String.Format ("asmb-{0}", i+1);
				comparray[i] = obj;
				evidence.AddAssembly (obj);
				AssertEquals (evidence.Count, i+1);
			}
			
			index = 0;
			foreach (object compobj in evidence) {
				AssertEquals ("Comparison object does not equal evidence assembly object", 
					comparray[index++], compobj);
			}
		}

		public void TestAddHost()
		{
			Evidence evidence = new Evidence ();
			object[] comparray = new object[100];
			string obj;
			int index;

			for (int i=0; i<100; i++) {
				obj = String.Format ("asmb-{0}", i+1);
				comparray[i] = obj;
				evidence.AddAssembly ( obj );
				AssertEquals (evidence.Count, i+1);
			}

			index = 0;
			foreach (object compobj in evidence) {
				AssertEquals ("Comparison object does not equal evidence host object", 
					comparray[index++], compobj);
			}
		}

		public void TestMultiArgConstructorForEach() 
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			ArrayList compare = new ArrayList (); 
			Evidence evidence = new Evidence (hostarray, asmbarray);
			int i;
		
			compare.AddRange (hostarray);
			compare.AddRange (asmbarray);
	
			i = 0;		
			foreach (object obj in evidence) {
				AssertEquals (obj, compare[i++]);
			}
		}

		public void TestEnumeratorReset() 
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			ArrayList compare = new ArrayList (); 
			Evidence evidence = new Evidence (hostarray, asmbarray);
			IEnumerator enumerator;
			int i;
			
			compare.AddRange (hostarray);
			compare.AddRange (asmbarray);

			i = 0;
			enumerator = evidence.GetEnumerator ();	
			while (enumerator.MoveNext ()) {
				AssertEquals (enumerator.Current, compare[i++]);
			}

			enumerator.Reset ();
			i = 0;
			while (enumerator.MoveNext ()) {
				AssertEquals (enumerator.Current, compare[i++]);
			}
		}

		public void TestGetHostEnumerator() 
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2" };
			Evidence evidence;
			IEnumerator enumerator;
			int i;

			evidence = new Evidence (hostarray, asmbarray);
			enumerator = evidence.GetHostEnumerator ();
			
			i = 0;
			while (enumerator.MoveNext ()) {
               			AssertEquals (enumerator.Current, hostarray[i++]);
			}
		}


		public void TestGetHostAssemblyEnumerator()
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

		public void TestCount() 
		{
			object[] hostarray = { "host-1", "host-2", "host-3", "host-4" };
			object[] asmbarray = { "asmb-1", "asmb-2", "asmb-3", "asmb-4" };
			Evidence evidence;
		
			evidence = new Evidence (hostarray, asmbarray);
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

		public void TestNullCopyToException() 
		{
			Evidence evidence;
	
			evidence = new Evidence ();
			evidence.AddHost ("host-1");
			
			try {
				evidence.CopyTo (null, 100);
				Fail ("CopyTo should throw exception when recieving a null array");
			} catch (Exception e) {
				Assert ("Should have caught an ArgumentNull Exception", e is ArgumentNullException);
			}
		}

		/// <summary>
		///    No Exception will be generated because the copy won't run because the evidence list is empty
		/// </summary>
		public void TestCopyToNoException() 
		{
			Evidence evidence = new Evidence ();;

			evidence.CopyTo (null, 100);
		}

		public void TestArgOutOfRangeCopyToException() 
		{
			Evidence evidence = new Evidence (new object[10], new object[10]);
			
			try {
				evidence.CopyTo (new object[10], -100);
				Fail ("CopyTo should throw exception when recieving a negative index");
			} catch (Exception e) {
				Assert("Should have caught an ArgumentOutOfRangeException Exception", 
					e is ArgumentOutOfRangeException);
			}
		}

		/// <summary>
		///    No Exception will be generated because the copy won't run because the evidence list is empty
		/// </summary>
		public void TestArgOutOfRangeCopyToNoException() 
		{
			Evidence evidence = new Evidence ();

			evidence.CopyTo (new object[10], -100);
		}

		public void BadMergeTest() {
			Evidence evidence, evidence2;
		
			evidence = new Evidence (null, null);
			evidence2 = new Evidence ();
		}

		public void MergeTest() {
			Evidence evidence, evidence2;

			evidence = new Evidence (new object[10], new object[10]);
		  	evidence2 = new Evidence ();

			evidence2.Merge (evidence);
		}
	}
}

