//
// CookieCollectionTest.cs - NUnit Test Cases for System.Net.CookieCollection
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;

namespace MonoTests.System.Net
{

public class CookieCollectionTest : TestCase
{
	CookieCollection col;
	
        public CookieCollectionTest () :
                base ("[MonoTests.System.Net.CookieCollectionTest]") {}

        public CookieCollectionTest (string name) : base (name) {}

        protected override void SetUp () 
        {
		col = new CookieCollection ();	
		col.Add (new Cookie ("name1", "value1"));
		col.Add (new Cookie ("name2", "value2", "path2"));
		col.Add (new Cookie ("name3", "value3", "path3", "domain3"));		
	}

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (CookieCollectionTest));
                }
        }
        
        public void TestCount ()
        {
		AssertEquals ("#1", col.Count, 3);
	}

        public void TestIndexer ()
        {
		Cookie c = null;
		try {
			c = col [-1];
			Fail ("#1");
		} catch (ArgumentOutOfRangeException) {
		}
		try {
			c = col [col.Count];
			Fail ("#2");
		} catch (ArgumentOutOfRangeException) {
		}
		c = col ["name1"];
		AssertEquals ("#3", c.Name, "name1");
		c = col ["NAME2"];
		AssertEquals ("#4", c.Name, "name2");
	}
	
	public void TestAdd ()
	{
		try {
			Cookie c = null;
			col.Add (c);
			Fail ("#1");
		} catch (ArgumentNullException) {
		}
		
		// in the microsoft implementation this will fail,
		// so we'll have to fail to.
		try {
			col.Add (col);
			Fail ("#2");
		} catch (Exception) {
		}
		AssertEquals ("#3", col.Count, 3);
		
		col.Add (new Cookie("name1", "value1"));		
		AssertEquals ("#4", col.Count, 3);
		
		CookieCollection col2 = new CookieCollection();
		Cookie c4 = new Cookie("name4", "value4");
		Cookie c5 = new Cookie("name5", "value5");
		col2.Add (c4);
		col2.Add (c5);
		col.Add (col2);
		AssertEquals ("#5", col.Count, 5);
		AssertEquals ("#6", col ["NAME4"], c4);
		AssertEquals ("#7", col [4], c5);
	}
	
	public void TestCopyTo ()
	{
		Array a = Array.CreateInstance (typeof (Cookie), 3);
		col.CopyTo (a, 0);
		AssertEquals ("#1", a.GetValue (0), col [0]);
		AssertEquals ("#2", a.GetValue (1), col [1]);
		AssertEquals ("#3", a.GetValue (2), col [2]);
	}
	
	public void TestEnumerator ()
	{
		IEnumerator enumerator = col.GetEnumerator ();
		enumerator.MoveNext ();
		Cookie c = (Cookie) enumerator.Current;
		AssertEquals ("#1", c, col [0]);
		col.Add (new Cookie ("name6", "value6"));
		try {
			enumerator.MoveNext ();
			Fail ("#2");
		} catch (InvalidOperationException) {
		}
	}
}

}

