//
// CookieCollectionTest.cs - NUnit Test Cases for System.Net.CookieCollection
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Net;
using System.Collections;

namespace MonoTests.System.Net
{

[TestFixture]
public class CookieCollectionTest
{
	CookieCollection col;
	
	[SetUp]
        public void GetReady () 
        {
		col = new CookieCollection ();	
		col.Add (new Cookie ("name1", "value1"));
		col.Add (new Cookie ("name2", "value2", "path2"));
		col.Add (new Cookie ("name3", "value3", "path3", "domain3"));		
	}

        [Test]
        public void Count ()
        {
		Assertion.AssertEquals ("#1", col.Count, 3);
	}

        [Test]
        public void Indexer ()
        {
		Cookie c = null;
		try {
			c = col [-1];
			Assertion.Fail ("#1");
		} catch (ArgumentOutOfRangeException) {
		}
		try {
			c = col [col.Count];
			Assertion.Fail ("#2");
		} catch (ArgumentOutOfRangeException) {
		}
		c = col ["name1"];
		Assertion.AssertEquals ("#3", c.Name, "name1");
		c = col ["NAME2"];
		Assertion.AssertEquals ("#4", c.Name, "name2");
	}
	
        [Test]
	public void Add ()
	{
		try {
			Cookie c = null;
			col.Add (c);
			Assertion.Fail ("#1");
		} catch (ArgumentNullException) {
		}
		
		// in the microsoft implementation this will fail,
		// so we'll have to fail to.
		try {
			col.Add (col);
			Assertion.Fail ("#2");
		} catch (Exception) {
		}
		Assertion.AssertEquals ("#3", col.Count, 3);
		
		col.Add (new Cookie("name1", "value1"));		
		Assertion.AssertEquals ("#4", col.Count, 3);
		
		CookieCollection col2 = new CookieCollection();
		Cookie c4 = new Cookie("name4", "value4");
		Cookie c5 = new Cookie("name5", "value5");
		col2.Add (c4);
		col2.Add (c5);
		col.Add (col2);
		Assertion.AssertEquals ("#5", col.Count, 5);
		Assertion.AssertEquals ("#6", col ["NAME4"], c4);
		Assertion.AssertEquals ("#7", col [4], c5);
	}
	
        [Test]
	public void CopyTo ()
	{
		Array a = Array.CreateInstance (typeof (Cookie), 3);
		col.CopyTo (a, 0);
		Assertion.AssertEquals ("#1", a.GetValue (0), col [0]);
		Assertion.AssertEquals ("#2", a.GetValue (1), col [1]);
		Assertion.AssertEquals ("#3", a.GetValue (2), col [2]);
	}
	
        [Test]
	public void Enumerator ()
	{
		IEnumerator enumerator = col.GetEnumerator ();
		enumerator.MoveNext ();
		Cookie c = (Cookie) enumerator.Current;
		Assertion.AssertEquals ("#1", c, col [0]);
		col.Add (new Cookie ("name6", "value6"));
		try {
			enumerator.MoveNext ();
			Assertion.Fail ("#2");
		} catch (InvalidOperationException) {
		}
	}
}
}

