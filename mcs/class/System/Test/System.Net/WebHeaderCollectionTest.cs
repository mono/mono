//
// WebHeaderCollectionTest.cs - NUnit Test Cases for System.Net.WebHeaderCollection
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
public class WebHeaderCollectionTest
{
	WebHeaderCollection col;
	
	[SetUp]
        public void GetReady () 
        {
		col = new WebHeaderCollection ();	
		col.Add ("Name1: Value1");
		col.Add ("Name2: Value2");
	}

        [Test]
        public void Add ()
        {
		try {
			col.Add (null);
			Assertion.Fail ("#1");
		} catch (ArgumentNullException) {}
		try {
			col.Add ("");
			Assertion.Fail ("#2");
		} catch (ArgumentException) {}
		try {
			col.Add ("  ");
			Assertion.Fail ("#3");
		} catch (ArgumentException) {}
		try {
			col.Add (":");
			Assertion.Fail ("#4");
		} catch (ArgumentException) {}
		try {
			col.Add (" : ");
			Assertion.Fail ("#5");
		} catch (ArgumentException) {}

		try {
			col.Add ("XHost: foo");
		} catch (ArgumentException) {
			Assertion.Fail ("#7");			
		}

		// invalid values
		try {
			col.Add ("XHost" + ((char) 0xa9) + ": foo");
			Assertion.Fail ("#8");
		} catch (ArgumentException) {}
		try {
			col.Add ("XHost: foo" + (char) 0xa9);
		} catch (ArgumentException) {
			Assertion.Fail ("#9");			
		}
		try {
			col.Add ("XHost: foo" + (char) 0x7f);
			Assertion.Fail ("#10");
		} catch (ArgumentException) {
			
		}

		try {
			col.Add ("XHost", null);
		} catch (ArgumentException) {
			Assertion.Fail ("#11");
		}		
		try {
			col.Add ("XHost:");			
		} catch (ArgumentException) {
			Assertion.Fail ("#12");			
		}				
		
		// restricted
		/*
		// this can only be tested in namespace System.Net
		try {
			WebHeaderCollection col2 = new WebHeaderCollection (true);
			col2.Add ("Host: foo");
			Assertion.Fail ("#13: should fail according to spec");
		} catch (ArgumentException) {}		
		*/
	}
	
        [Test]
	public void GetValues ()
	{			
		WebHeaderCollection w = new WebHeaderCollection ();
		w.Add ("Hello", "H1");
		w.Add ("Hello", "H2");
		w.Add ("Hello", "H3,H4");
		
		string [] sa = w.GetValues ("Hello");
		Assertion.AssertEquals ("#1", 3, sa.Length);
		Assertion.AssertEquals ("#2", "H1,H2,H3,H4", w.Get ("Hello"));

		w = new WebHeaderCollection ();
		w.Add ("Accept", "H1");
		w.Add ("Accept", "H2");
		w.Add ("Accept", "H3,H4");		
		Assertion.AssertEquals ("#3a", 3, w.GetValues (0).Length);
		Assertion.AssertEquals ("#3b", 4, w.GetValues ("Accept").Length);
		Assertion.AssertEquals ("#4", "H1,H2,H3,H4", w.Get ("Accept"));

		w = new WebHeaderCollection ();
		w.Add ("Allow", "H1");
		w.Add ("Allow", "H2");
		w.Add ("Allow", "H3,H4");		
		sa = w.GetValues ("Allow");		
		Assertion.AssertEquals ("#5", 4, sa.Length);
		Assertion.AssertEquals ("#6", "H1,H2,H3,H4", w.Get ("Allow"));

		w = new WebHeaderCollection ();
		w.Add ("AUTHorization", "H1, H2, H3");
		sa = w.GetValues ("authorization");		
		Assertion.AssertEquals ("#9", 3, sa.Length);

		w = new WebHeaderCollection ();
		w.Add ("proxy-authenticate", "H1, H2, H3");
		sa = w.GetValues ("Proxy-Authenticate");		
		Assertion.AssertEquals ("#9", 3, sa.Length);

		w = new WebHeaderCollection ();
		w.Add ("expect", "H1,\tH2,   H3  ");
		sa = w.GetValues ("EXPECT");		
		Assertion.AssertEquals ("#10", 3, sa.Length);
		Assertion.AssertEquals ("#11", "H2", sa [1]);
		Assertion.AssertEquals ("#12", "H3", sa [2]);
		
		try {
			w.GetValues (null);
			Assertion.Fail ("#13");
		} catch (ArgumentNullException) {}
		Assertion.AssertEquals ("#14", null, w.GetValues (""));
		Assertion.AssertEquals ("#15", null, w.GetValues ("NotExistent"));
	}
	
        [Test]
	public void Indexers ()
	{
		Assertion.AssertEquals ("#1", "Value1", col [0]);
		Assertion.AssertEquals ("#2", "Value1", col ["Name1"]);
		Assertion.AssertEquals ("#3", "Value1", col ["NAME1"]);
	}

	[Test]
	public void Remove ()
	{
		col.Remove ("Name1");
		col.Remove ("NameNotExist");
		Assertion.AssertEquals ("#1", 1, col.Count);
		
		/*
		// this can only be tested in namespace System.Net
		try {
			WebHeaderCollection col2 = new WebHeaderCollection (true);
			col2.Add ("Host", "foo");
			col2.Remove ("Host");
			Assertion.Fail ("#2: should fail according to spec");
		} catch (ArgumentException) {}
		*/
	}

        [Test]	
	public void Set ()
	{
		col.Add ("Name1", "Value1b");
		col.Set ("Name1", "\t  X  \t");
		Assertion.AssertEquals ("#1", "X", col.Get ("Name1"));
	}
	
        [Test]
	public void IsRestricted ()
	{
		Assertion.Assert ("#1", !WebHeaderCollection.IsRestricted ("Xhost"));
		Assertion.Assert ("#2", WebHeaderCollection.IsRestricted ("Host"));
		Assertion.Assert ("#3", WebHeaderCollection.IsRestricted ("HOST"));
		Assertion.Assert ("#4", WebHeaderCollection.IsRestricted ("Transfer-Encoding"));
		Assertion.Assert ("#5", WebHeaderCollection.IsRestricted ("user-agent"));
		Assertion.Assert ("#6", WebHeaderCollection.IsRestricted ("accept"));
		Assertion.Assert ("#7", !WebHeaderCollection.IsRestricted ("accept-charset"));
	}

	[Test]
	public void ToStringTest ()
	{
		col.Add ("Name1", "Value1b");
		col.Add ("Name3", "Value3a\r\n Value3b");
		col.Add ("Name4", "   Value4   ");
		Assertion.AssertEquals ("#1", "Name1: Value1,Value1b\r\nName2: Value2\r\nName3: Value3a\r\n Value3b\r\nName4: Value4\r\n\r\n", col.ToString ());
	}
}

}

