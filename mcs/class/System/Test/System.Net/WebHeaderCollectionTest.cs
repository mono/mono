//
// WebHeaderCollectionTest.cs - NUnit Test Cases for System.Net.WebHeaderCollection
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

public class WebHeaderCollectionTest : TestCase
{
	WebHeaderCollection col;
	
        public WebHeaderCollectionTest () :
                base ("[MonoTests.System.Net.WebHeaderCollectionTest]") {}

        public WebHeaderCollectionTest (string name) : base (name) {}

        protected override void SetUp () 
        {
		col = new WebHeaderCollection ();	
		col.Add ("Name1: Value1");
		col.Add ("Name2: Value2");
	}

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (WebHeaderCollectionTest));
                }
        }
        
        public void TestAdd ()
        {
		try {
			col.Add (null);
			Fail ("#1");
		} catch (ArgumentNullException) {}
		try {
			col.Add ("");
			Fail ("#2");
		} catch (ArgumentException) {}
		try {
			col.Add ("  ");
			Fail ("#3");
		} catch (ArgumentException) {}
		try {
			col.Add (":");
			Fail ("#4");
		} catch (ArgumentException) {}
		try {
			col.Add (" : ");
			Fail ("#5");
		} catch (ArgumentException) {}

		try {
			col.Add ("XHost: foo");
		} catch (ArgumentException) {
			Fail ("#7");			
		}

		// invalid values
		try {
			col.Add ("XHost" + ((char) 0xa9) + ": foo");
			Fail ("#8");
		} catch (ArgumentException) {}
		try {
			col.Add ("XHost: foo" + (char) 0xa9);
		} catch (ArgumentException) {
			Fail ("#9");			
		}
		try {
			col.Add ("XHost: foo" + (char) 0x7f);
			Fail ("#10");
		} catch (ArgumentException) {
			
		}

		try {
			col.Add ("XHost", null);
		} catch (ArgumentException) {
			Fail ("#11");
		}		
		try {
			col.Add ("XHost:");			
		} catch (ArgumentException) {
			Fail ("#12");			
		}				
		
		// restricted
		/*
		// this can only be tested in namespace System.Net
		try {
			WebHeaderCollection col2 = new WebHeaderCollection (true);
			col2.Add ("Host: foo");
			Fail ("#13: should fail according to spec");
		} catch (ArgumentException) {}		
		*/
	}
	
	public void TestGetValues ()
	{			
		WebHeaderCollection w = new WebHeaderCollection ();
		w.Add ("Hello", "H1");
		w.Add ("Hello", "H2");
		w.Add ("Hello", "H3,H4");
		
		string [] sa = w.GetValues ("Hello");
		AssertEquals ("#1", 3, sa.Length);
		AssertEquals ("#2", "H1,H2,H3,H4", w.Get ("Hello"));

		w = new WebHeaderCollection ();
		w.Add ("Accept", "H1");
		w.Add ("Accept", "H2");
		w.Add ("Accept", "H3,H4");		
		AssertEquals ("#3a", 3, w.GetValues (0).Length);
		AssertEquals ("#3b", 4, w.GetValues ("Accept").Length);
		AssertEquals ("#4", "H1,H2,H3,H4", w.Get ("Accept"));

		w = new WebHeaderCollection ();
		w.Add ("Allow", "H1");
		w.Add ("Allow", "H2");
		w.Add ("Allow", "H3,H4");		
		sa = w.GetValues ("Allow");		
		AssertEquals ("#5", 4, sa.Length);
		AssertEquals ("#6", "H1,H2,H3,H4", w.Get ("Allow"));

		w = new WebHeaderCollection ();
		w.Add ("AUTHorization", "H1, H2, H3");
		sa = w.GetValues ("authorization");		
		AssertEquals ("#9", 3, sa.Length);

		w = new WebHeaderCollection ();
		w.Add ("proxy-authenticate", "H1, H2, H3");
		sa = w.GetValues ("Proxy-Authenticate");		
		AssertEquals ("#9", 3, sa.Length);

		w = new WebHeaderCollection ();
		w.Add ("expect", "H1,\tH2,   H3  ");
		sa = w.GetValues ("EXPECT");		
		AssertEquals ("#10", 3, sa.Length);
		AssertEquals ("#11", "H2", sa [1]);
		AssertEquals ("#12", "H3", sa [2]);
		
		try {
			w.GetValues (null);
			Fail ("#13");
		} catch (ArgumentNullException) {}
		AssertEquals ("#14", null, w.GetValues (""));
		AssertEquals ("#15", null, w.GetValues ("NotExistent"));
	}
	
	public void TestIndexers ()
	{
		AssertEquals ("#1", "Value1", col [0]);
		AssertEquals ("#2", "Value1", col ["Name1"]);
		AssertEquals ("#3", "Value1", col ["NAME1"]);
	}
	
	public void TestRemove ()
	{
		col.Remove ("Name1");
		col.Remove ("NameNotExist");
		AssertEquals ("#1", 1, col.Count);
		
		/*
		// this can only be tested in namespace System.Net
		try {
			WebHeaderCollection col2 = new WebHeaderCollection (true);
			col2.Add ("Host", "foo");
			col2.Remove ("Host");
			Fail ("#2: should fail according to spec");
		} catch (ArgumentException) {}
		*/
	}
	
	public void TestSet ()
	{
		col.Add ("Name1", "Value1b");
		col.Set ("Name1", "\t  X  \t");
		AssertEquals ("#1", "X", col.Get ("Name1"));
	}
	
	public void TestIsRestricted ()
	{
		Assert ("#1", !WebHeaderCollection.IsRestricted ("Xhost"));
		Assert ("#2", WebHeaderCollection.IsRestricted ("Host"));
		Assert ("#3", WebHeaderCollection.IsRestricted ("HOST"));
		Assert ("#4", WebHeaderCollection.IsRestricted ("Transfer-Encoding"));
		Assert ("#5", WebHeaderCollection.IsRestricted ("user-agent"));
		Assert ("#6", WebHeaderCollection.IsRestricted ("accept"));
		Assert ("#7", !WebHeaderCollection.IsRestricted ("accept-charset"));
	}
	
	public void TestToString ()
	{
		col.Add ("Name1", "Value1b");
		col.Add ("Name3", "Value3a\r\n Value3b");
		col.Add ("Name4", "   Value4   ");
		AssertEquals ("#1", "Name1: Value1,Value1b\r\nName2: Value2\r\nName3: Value3a\r\n Value3b\r\nName4: Value4\r\n\r\n", col.ToString ());
	}
}

}

