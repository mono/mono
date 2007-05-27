// created on 7/21/2001 at 2:36 PM
//
// Authors:
//	Martin Willemoes Hansen (mwh@sysrq.dk)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
			AssertEquals ("#1", null, col.GetValues (null));
			AssertEquals ("#2", null, col.GetValues (""));
			AssertEquals ("#3", null, col.GetValues ("NotExistent"));
			AssertEquals ("#4", 1, col.GetValues (0).Length);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetValues_OutOfRange ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("foo1", "bar1");
			AssertEquals ("#5", null, c.GetValues (1));
		}

		[Test]
		public void Get ()
		{
			NameValueCollection col = new NameValueCollection (5);
			col.Add ("foo1", "bar1");
			AssertEquals ("#1", null, col.Get (null));
			AssertEquals ("#2", null, col.Get (""));
			AssertEquals ("#3", null, col.Get ("NotExistent"));
			AssertEquals ("#4", "bar1", col.Get ("foo1"));
			AssertEquals ("#5", "bar1", col.Get (0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Get_OutOfRange ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("foo1", "bar1");
			AssertEquals ("#6", null, c.Get (1));
		}

		[Test]
		public void GetKey ()
		{
			NameValueCollection c = new NameValueCollection (CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
			c.Add ("foo1", "bar1");
			AssertEquals ("#1", "foo1", c.GetKey (0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetKey_OutOfRange ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("foo1", "bar1");
			AssertEquals ("#2", null, c.GetKey (1));
		}

		[Test]
		public void HasKeys ()
		{
			NameValueCollection c = new NameValueCollection (5, CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
			Assert ("#1", !c.HasKeys ());
			c.Add ("foo1", "bar1");
			Assert ("#2", c.HasKeys ());
		}

		[Test]
		public void Clear ()
		{
			NameValueCollection c = new NameValueCollection ();
			AssertEquals ("#1", 0, c.Count);
			c.Add ("foo1", "bar1");
			AssertEquals ("#2", 1, c.Count);
			c.Clear ();
			AssertEquals ("#3", 0, c.Count);
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
			NameValueCollection c2 = new NameValueCollection (c1);

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
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void Add_NVC_Null ()
		{
			new NameValueCollection ().Add (null);
		}

		[Test]
		public void Add_NVC_Null2 ()
		{
			NameValueCollection a = new NameValueCollection ();
			NameValueCollection b = new NameValueCollection ();

			b.Add ("Test", null);
			a.Add (b);
			AssertEquals ("Count", 1, a.Count);
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
#if NET_2_0
		[ExpectedException (typeof (InvalidCastException))]
#else		
		[ExpectedException (typeof (ArrayTypeMismatchException))]
#endif
		public void CopyTo_WrongTypeArray ()
		{
			Array a = Array.CreateInstance (typeof (DateTime), 3);
			NameValueCollection c = new NameValueCollection ();
			for (int i = 0; i < 3; i++)
				c.Add(i.ToString(), i.ToString());
			c.CopyTo(a, 0);
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
		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif		
		public void Constructor_Null_NVC ()
		{
			NameValueCollection nvc = new NameValueCollection((NameValueCollection)null);
		}
		
		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif		
		public void Constructor_Capacity_Null_NVC ()
		{
			NameValueCollection nvc = new NameValueCollection(10, (NameValueCollection)null);
		}

#if NET_2_0
		[Test]
		public void Constructor_IEqualityComparer ()
		{
			NameValueCollection coll = new NameValueCollection (new EqualityComparer ());
			coll.Add ("a", "1");
			AssertEquals ("#1", 1, coll.Count);
		}

		[Test]
		public void Constructor_Int_IEqualityComparer ()
		{
			NameValueCollection coll = new NameValueCollection (5, new EqualityComparer ());
			coll.Add ("a", "1");
			AssertEquals ("#1", 1, coll.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_IntNegative_IEqualityComparer ()
		{
			new NameValueCollection (-1, new EqualityComparer ());
		}

		[Test]
		public void Constructor_IEqualityComparer_Null ()
		{
			NameValueCollection c1 = new NameValueCollection ((IEqualityComparer)null);
			c1.Add ("key", "value");
			AssertEquals ("Constructor_IEqualityComparer_Null", c1.Get ("KEY"), "value");
			c1.Remove ("key");
		}

		[Test]
		public void Constructor_NameValueCollection ()
		{
			NameValueCollection c1 = new NameValueCollection (StringComparer.InvariantCultureIgnoreCase);
			c1.Add ("key", "value");
			NameValueCollection c2 = new NameValueCollection (c1);
			AssertEquals ("Constructor_NameValueCollection", c2.Get ("KEY"), "value");
			c2.Remove ("key");
		}
#endif
	}
}
