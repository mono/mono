// created on 7/21/2001 at 2:36 PM
//
// Authors:
//	Martin Willemoes Hansen (mwh@sysrq.dk)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
        public class NameValueCollectionTest : Assertion {

		[Test]
		public void GetValues ()
		{
			NameValueCollection col = new NameValueCollection ();
			col.Add ("foo1", "bar1");
			Assertion.AssertEquals ("#1", null, col.GetValues (null));
			Assertion.AssertEquals ("#2", null, col.GetValues (""));
			Assertion.AssertEquals ("#3", null, col.GetValues ("NotExistent"));
		}

		[Test]
		public void Add ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("!mono", null);
			c.Add (null, "mono!");
			AssertEquals ("Count", 3, c.Count);
			AssertEquals ("mono", "mono", c ["mono"]);
			AssertNull ("!mono", c ["!mono"]);
			AssertEquals ("mono!", "mono!", c [null]);
		}

		[Test]
		public void Add_Multiples ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("mono", "mono");
			c.Add ("mono", "mono");
			AssertEquals ("Count", 1, c.Count);
			AssertEquals ("mono", "mono,mono,mono", c ["mono"]);
		}

		[Test]
		public void Add_Multiples_Null ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("mono", null);
			c.Add ("mono", "mono");
			AssertEquals ("Count", 1, c.Count);
			AssertEquals ("mono", "mono,mono", c ["mono"]);
		}

		[Test]
		public void Add_NVC ()
		{
			NameValueCollection c1 = new NameValueCollection ();
			NameValueCollection c2 = new NameValueCollection ();

			c2.Add (c1);
			AssertEquals ("c1.Count", 0, c1.Count);
			AssertEquals ("c2.Count", 0, c2.Count);

			c1.Add ("foo", "bar");
			c2.Add ("bar", "foo");

			AssertEquals ("c1.Count", 1, c1.Count);
			AssertEquals ("c2.Count", 1, c2.Count);

			c2.Add (c1);
			AssertEquals ("c1.Count", 1, c1.Count);
			AssertEquals ("c2.Count", 2, c2.Count);
		}

		[Test]
//		[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void Add_NVC_Null ()
		{
			new NameValueCollection ().Add (null);
		}

		[Test]
		public void Set_New ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Set ("mono", "mono");
			c.Set ("!mono", null);
			c.Set (null, "mono!");
			AssertEquals ("Count", 3, c.Count);
			AssertEquals ("mono", "mono", c ["mono"]);
			AssertNull ("!mono", c ["!mono"]);
			AssertEquals ("mono!", "mono!", c [null]);
		}

		[Test]
		public void Set_Replace ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("!mono", "!mono");
			c.Add ("mono!", "mono!");
			AssertEquals ("Count", 3, c.Count);
			AssertEquals ("mono", "mono", c ["mono"]);
			AssertEquals ("!mono", "!mono", c ["!mono"]);
			AssertEquals ("mono!", "mono!", c ["mono!"]);

			c.Set ("mono", "nomo");
			c.Set ("!mono", null);
			c.Set (null, "mono!");
			AssertEquals ("Count", 4, c.Count); // mono! isn't removed
			AssertEquals ("mono", "nomo", c ["mono"]);
			AssertNull ("!mono", c ["!mono"]);
			AssertEquals ("mono!1", "mono!", c ["mono!"]);
			AssertEquals ("mono!2", "mono!", c [null]);
		}

		[Test]
		public void CaseInsensitive () 
		{
			// default constructor is case insensitive
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("MoNo", "MoNo");
			c.Add ("mOnO", "mOnO");
			c.Add ("MONO", "MONO");
			AssertEquals ("Count", 1, c.Count);
		}

		[Test]
		public void CopyTo () 
		{
			string [] array = new string [4];
			NameValueCollection c = new NameValueCollection ();
			c.Add ("1", "mono");
			c.Add ("2", "MoNo");
			c.Add ("3", "mOnO");
			c.Add ("4", "MONO");
			c.CopyTo (array, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyTo_Null () 
		{
			NameValueCollection c = new NameValueCollection ();
			c.CopyTo (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CopyTo_NegativeIndex () 
		{
			string [] array = new string [4];
			NameValueCollection c = new NameValueCollection ();
			c.Add ("1", "mono");
			c.Add ("2", "MoNo");
			c.Add ("3", "mOnO");
			c.Add ("4", "MONO");
			c.CopyTo (array, -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_NotEnoughSpace () 
		{
			string [] array = new string [4];
			NameValueCollection c = new NameValueCollection ();
			c.Add ("1", "mono");
			c.Add ("2", "MoNo");
			c.Add ("3", "mOnO");
			c.Add ("4", "MONO");
			c.CopyTo (array, 2);
		}

		[Test]
		// Note: not a RankException
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_MultipleDimensionStringArray () 
		{
			string [,,] matrix = new string [2,3,4];
			NameValueCollection c = new NameValueCollection ();
			c.Add ("1", "mono");
			c.Add ("2", "MoNo");
			c.Add ("3", "mOnO");
			c.Add ("4", "MONO");
			c.CopyTo (matrix, 0);
		}

		[Test]
		// Note: not a RankException
		[ExpectedException (typeof (ArgumentException))]
		public void CopyTo_MultipleDimensionArray () 
		{
			Array a = Array.CreateInstance (typeof (string), 1, 2, 3);
			NameValueCollection c = new NameValueCollection ();
			c.CopyTo (a, 0);
		}

		[Test]
		public void Remove () 
		{
			string[] items = { "mono", "MoNo", "mOnO", "MONO" };
			// default constructor is case insensitive
			NameValueCollection c = new NameValueCollection ();
			for (int i=0; i < items.Length; i++) {
				string add = "Add-" + i.ToString () + "-Count";

				c.Add (items [i], add);
				AssertEquals (add, 1, c.Count);
				c.Remove (items [0]);
				AssertEquals ("Remove-0-Count", 0, c.Count);

				c.Add (items [i], add);
				AssertEquals (add, 1, c.Count);
				c.Remove (items [1]);
				AssertEquals ("Remove-1-Count", 0, c.Count);

				c.Add (items [i], add);
				AssertEquals (add, 1, c.Count);
				c.Remove (items [2]);
				AssertEquals ("Remove-2-Count", 0, c.Count);

				c.Add (items [i], add);
				AssertEquals (add , 1, c.Count);
				c.Remove (items [3]);
				AssertEquals ("Remove-3-Count", 0, c.Count);
			}
		}
	}
}
