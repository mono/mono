// CaseInsensitiveComparerTest

using System;
using System.Collections;

using NUnit.Framework;



namespace MonoTests.System.Collections {


	/// <summary>CaseInsensitiveComparer test suite.</summary>
        [TestFixture]
	public class CaseInsensitiveComparerTest : TestCase {
		protected override void SetUp ()
		{
		}

		[Test]
		public void TestDefaultInstance ()
		{
			// Make sure the instance returned by Default
			// is really a CaseInsensitiveComparer.
			Assert((CaseInsensitiveComparer.Default
			        as CaseInsensitiveComparer) != null);
		}

		[Test]
		public void TestCompare () {
			CaseInsensitiveComparer cic = new CaseInsensitiveComparer ();

			AssertEquals(cic.Compare ("WILD WEST", "Wild West"),0);
			AssertEquals(cic.Compare ("WILD WEST", "wild west"),0);
			Assert(cic.Compare ("Zeus", "Mars") > 0);
			Assert(cic.Compare ("Earth", "Venus") < 0);
		}

		[Test]
		public void TestIntsNEq()
		{
			int a =1;
			int b =2;                                   
			AssertEquals("#01",Comparer.Default.Compare(a,b),CaseInsensitiveComparer.Default.Compare(a,b));
		}
		
		[Test]
		public void TestIntsEq()
		{
			int a =1;
			int b =1;                                                                                
		
			AssertEquals("#02",Comparer.Default.Compare(a,b),CaseInsensitiveComparer.Default.Compare(a,b));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNull()
		{
		    new CaseInsensitiveComparer(null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestObject()
		{
		    object a = new object();
		    object b = new object();
		    CaseInsensitiveComparer.Default.Compare(a,b);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestDiffArgs()
		{
		    int a = 5;
		    string b = "hola";
		    CaseInsensitiveComparer.Default.Compare(a,b);
		}

		[Test]
		public void TestNull1()
		{
			string a = null;
			string b = "5";

			AssertEquals("#04 Failed",Comparer.Default.Compare(a,b),CaseInsensitiveComparer.Default.Compare(a,b));
		}

		[Test]
		public void TestNull2()
		{
			string a = null;
			string b = null;

			AssertEquals("#05 Failed",Comparer.Default.Compare(a,b),CaseInsensitiveComparer.Default.Compare(a,b));
		}
		
		[Test]
		public void TestStringsCaps()
		{
			string a = "AA";
			string b = "aa";

			AssertEquals("#06 Failed",CaseInsensitiveComparer.Default.Compare(a,b),0);
		}
      
	}

}
