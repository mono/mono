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
		Assert.AreEqual (col.Count, 3, "#1");
	}

        [Test]
        public void Indexer ()
        {
		Cookie c = null;
		try {
			c = col [-1];
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException) {
		}
		try {
			c = col [col.Count];
			Assert.Fail ("#2");
		} catch (ArgumentOutOfRangeException) {
		}
		c = col ["name1"];
		Assert.AreEqual (c.Name, "name1", "#3");
		c = col ["NAME2"];
		Assert.AreEqual (c.Name, "name2", "#4");
	}
	
        [Test]
	public void Add ()
	{
		try {
			Cookie c = null;
			col.Add (c);
			Assert.Fail ("#1");
		} catch (ArgumentNullException) {
		}
		
		// in the microsoft implementation this will fail,
		// so we'll have to fail to.
		try {
			col.Add (col);
			Assert.Fail ("#2");
		} catch (Exception) {
		}
		Assert.AreEqual (col.Count, 3, "#3");
		
		col.Add (new Cookie("name1", "value1"));		
		Assert.AreEqual (col.Count, 3, "#4");
		
		CookieCollection col2 = new CookieCollection();
		Cookie c4 = new Cookie("name4", "value4");
		Cookie c5 = new Cookie("name5", "value5");
		col2.Add (c4);
		col2.Add (c5);
		col.Add (col2);
		Assert.AreEqual (col.Count, 5, "#5");
		Assert.AreEqual (col ["NAME4"], c4, "#6");
		Assert.AreEqual (col [4], c5, "#7");
	}
	
        [Test]
	public void CopyTo ()
	{
		Array a = Array.CreateInstance (typeof (Cookie), 3);
		col.CopyTo (a, 0);
		Assert.AreEqual (a.GetValue (0), col [0], "#1");
		Assert.AreEqual (a.GetValue (1), col [1], "#2");
		Assert.AreEqual (a.GetValue (2), col [2], "#3");
	}
	
        [Test]
	public void Enumerator ()
	{
		IEnumerator enumerator = col.GetEnumerator ();
		enumerator.MoveNext ();
		Cookie c = (Cookie) enumerator.Current;
		Assert.AreEqual (c, col [0], "#1");
		col.Add (new Cookie ("name6", "value6"));
		try {
			enumerator.MoveNext ();
			Assert.Fail ("#2");
		} catch (InvalidOperationException) {
		}
	}
}
}

