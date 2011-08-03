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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
        public class NameValueCollectionTest {

		[Test]
		public void GetValues ()
		{
			NameValueCollection col = new NameValueCollection ();
			col.Add ("foo1", "bar1");
			Assert.AreEqual (null, col.GetValues (null), "#1");
			Assert.AreEqual (null, col.GetValues (""), "#2");
			Assert.AreEqual (null, col.GetValues ("NotExistent"), "#3");
			Assert.AreEqual (1, col.GetValues (0).Length, "#4");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetValues_OutOfRange ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("foo1", "bar1");
			Assert.AreEqual (null, c.GetValues (1), "#5");
		}

		[Test]
		public void Get ()
		{
			NameValueCollection col = new NameValueCollection (5);
			col.Add ("foo1", "bar1");
			Assert.AreEqual (null, col.Get (null), "#1");
			Assert.AreEqual (null, col.Get (""), "#2");
			Assert.AreEqual (null, col.Get ("NotExistent"), "#3");
			Assert.AreEqual ("bar1", col.Get ("foo1"), "#4");
			Assert.AreEqual ("bar1", col.Get (0), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Get_OutOfRange ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("foo1", "bar1");
			Assert.AreEqual (null, c.Get (1), "#6");
		}

		[Test]
		public void GetKey ()
		{
			NameValueCollection c = new NameValueCollection (CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
			c.Add ("foo1", "bar1");
			Assert.AreEqual ("foo1", c.GetKey (0), "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetKey_OutOfRange ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("foo1", "bar1");
			Assert.AreEqual (null, c.GetKey (1), "#2");
		}

		[Test]
		public void HasKeys ()
		{
			NameValueCollection c = new NameValueCollection (5, CaseInsensitiveHashCodeProvider.DefaultInvariant, CaseInsensitiveComparer.DefaultInvariant);
			Assert.IsTrue (!c.HasKeys (), "#1");
			c.Add ("foo1", "bar1");
			Assert.IsTrue (c.HasKeys (), "#2");
		}

		[Test]
		public void Clear ()
		{
			NameValueCollection c = new NameValueCollection ();
			Assert.AreEqual (0, c.Count, "#1");
			c.Add ("foo1", "bar1");
			Assert.AreEqual (1, c.Count, "#2");
			c.Clear ();
			Assert.AreEqual (0, c.Count, "#3");
		}

		[Test]
		public void Add ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("!mono", null);
			c.Add (null, "mono!");
			Assert.AreEqual (3, c.Count, "Count");
			Assert.AreEqual ("mono", c ["mono"], "mono");
			Assert.IsNull (c ["!mono"], "!mono");
			Assert.AreEqual ("mono!", c [null], "mono!");
		}

		[Test]
		public void Add_Calls ()
		{
			var nvc1 = new MyNVC ();
			var nvc2 = new MyNVC ();
			nvc1.Add ("one", "1");
			nvc1.Add ("one", "one");
			nvc1.Add ("two", null);
			nvc2.Add (nvc1);

			string[] values;
			Assert.AreEqual (8, nvc1.Log.Count, "#A1-1");
			Assert.AreEqual ("Add (string, string)", nvc1.Log [0], "#A1-2");
			Assert.AreEqual ("Add (string, string)", nvc1.Log [1], "#A1-3");
			Assert.AreEqual ("Add (string, string)", nvc1.Log [2], "#A1-4");
			Assert.AreEqual ("get_Count", nvc1.Log [3], "#A1-5");
			Assert.AreEqual ("GetKey (int)", nvc1.Log [4], "#A1-6");
			Assert.AreEqual ("GetValues (int)", nvc1.Log [5], "#A1-7");
			Assert.AreEqual ("GetKey (int)", nvc1.Log [6], "#A1-8");
			Assert.AreEqual ("GetValues (int)", nvc1.Log [7], "#A1-9");

			Assert.AreEqual (2, nvc1.Count, "#A2-1");
			values = nvc1.GetValues (0);
			Assert.AreEqual ("one", nvc1.GetKey (0), "#A2-2");
			Assert.AreEqual ("1", values [0], "#A2-3");
			Assert.AreEqual ("one", values [1], "#A2-4");
			values = nvc1.GetValues (1);
			Assert.AreEqual ("two", nvc1.GetKey (1), "#A2-5");
			Assert.IsTrue (values == null, "#A2-6");

			Assert.AreEqual (3, nvc2.Log.Count, "#B1-1");
			Assert.AreEqual ("Add (string, string)", nvc2.Log [0], "#B1-2");
			Assert.AreEqual ("Add (string, string)", nvc2.Log [1], "#B1-3");
			Assert.AreEqual ("Add (string, string)", nvc2.Log [2], "#B1-4");

			Assert.AreEqual (2, nvc2.Count, "#B2-1");
			values = nvc2.GetValues (0);
			Assert.AreEqual ("one", nvc2.GetKey (0), "#B2-2");
			Assert.AreEqual ("1", values [0], "#B2-3");
			Assert.AreEqual ("one", values [1], "#B2-4");
			values = nvc2.GetValues (1);
			Assert.AreEqual ("two", nvc2.GetKey (1), "#B2-5");
			Assert.IsTrue (values == null, "#B2-6");
		}
		
		[Test]
		public void Add_Multiples ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("mono", "mono");
			c.Add ("mono", "mono");
			Assert.AreEqual (1, c.Count, "Count");
			Assert.AreEqual ("mono,mono,mono", c ["mono"], "mono");
		}

		[Test]
		public void Add_Multiples_Null ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("mono", null);
			c.Add ("mono", "mono");
			Assert.AreEqual (1, c.Count, "Count");
			Assert.AreEqual ("mono,mono", c ["mono"], "mono");
		}

		[Test]
		public void Add_NVC ()
		{
			NameValueCollection c1 = new NameValueCollection ();
			NameValueCollection c2 = new NameValueCollection (c1);

			c2.Add (c1);
			Assert.AreEqual (0, c1.Count, "c1.Count");
			Assert.AreEqual (0, c2.Count, "c2.Count");

			c1.Add ("foo", "bar");
			c2.Add ("bar", "foo");

			Assert.AreEqual (1, c1.Count, "c1.Count");
			Assert.AreEqual (1, c2.Count, "c2.Count");

			c2.Add (c1);
			Assert.AreEqual (1, c1.Count, "c1.Count");
			Assert.AreEqual (2, c2.Count, "c2.Count");
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
			Assert.AreEqual (1, a.Count, "Count");
		}

		[Test]
		public void Set_New ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Set ("mono", "mono");
			c.Set ("!mono", null);
			c.Set (null, "mono!");
			Assert.AreEqual (3, c.Count, "Count");
			Assert.AreEqual ("mono", c ["mono"], "mono");
			Assert.IsNull (c ["!mono"], "!mono");
			Assert.AreEqual ("mono!", c [null], "mono!");
		}

		[Test]
		public void Set_Replace ()
		{
			NameValueCollection c = new NameValueCollection ();
			c.Add ("mono", "mono");
			c.Add ("!mono", "!mono");
			c.Add ("mono!", "mono!");
			Assert.AreEqual (3, c.Count, "Count");
			Assert.AreEqual ("mono", c ["mono"], "mono");
			Assert.AreEqual ("!mono", c ["!mono"], "!mono");
			Assert.AreEqual ("mono!", c ["mono!"], "mono!");

			c.Set ("mono", "nomo");
			c.Set ("!mono", null);
			c.Set (null, "mono!");
			Assert.AreEqual (4, c.Count, "Count"); // mono! isn't removed
			Assert.AreEqual ("nomo", c ["mono"], "mono");
			Assert.IsNull (c ["!mono"], "!mono");
			Assert.AreEqual ("mono!", c ["mono!"], "mono!1");
			Assert.AreEqual ("mono!", c [null], "mono!2");
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
			Assert.AreEqual (1, c.Count, "Count");
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
				Assert.AreEqual (1, c.Count, add);
				c.Remove (items [0]);
				Assert.AreEqual (0, c.Count, "Remove-0-Count");

				c.Add (items [i], add);
				Assert.AreEqual (1, c.Count, add);
				c.Remove (items [1]);
				Assert.AreEqual (0, c.Count, "Remove-1-Count");

				c.Add (items [i], add);
				Assert.AreEqual (1, c.Count, add);
				c.Remove (items [2]);
				Assert.AreEqual (0, c.Count, "Remove-2-Count");

				c.Add (items [i], add);
				Assert.AreEqual (1, c.Count, add);
				c.Remove (items [3]);
				Assert.AreEqual (0, c.Count, "Remove-3-Count");
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
			Assert.AreEqual (1, coll.Count, "#1");
		}

		[Test]
		public void Constructor_Int_IEqualityComparer ()
		{
			NameValueCollection coll = new NameValueCollection (5, new EqualityComparer ());
			coll.Add ("a", "1");
			Assert.AreEqual (1, coll.Count, "#1");
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
			Assert.AreEqual (c1.Get ("KEY"), "value", "Constructor_IEqualityComparer_Null");
			c1.Remove ("key");
		}

		[Test]
		public void Constructor_NameValueCollection ()
		{
			NameValueCollection c1 = new NameValueCollection (StringComparer.InvariantCultureIgnoreCase);
			c1.Add ("key", "value");
			NameValueCollection c2 = new NameValueCollection (c1);
			Assert.AreEqual (c2.Get ("KEY"), "value", "Constructor_NameValueCollection");
			c2.Remove ("key");
		}
#endif
		class MyNVC : NameValueCollection
		{
			List<string> log;

			public List<string> Log {
				get {
					if (log == null)
						log = new List<string> ();
					return log;
				}
			}

			public override KeysCollection Keys {
				get {
					Log.Add ("get_Keys");
					return base.Keys;
				}
			}

			public override int Count {
				get {
					Log.Add ("get_Count");
					return base.Count;
				}
			}

			public override string[] AllKeys {
				get {
					Log.Add ("get_AllKeys");
					return base.AllKeys;
				}
			}
			
			public override string Get (int index)
			{
				Log.Add ("Get (int)");
				return base.Get (index);
			}

			public override string Get (string name)
			{
				Log.Add ("Get (string)");
				return base.Get (name);
			}

			public override string GetKey (int index)
			{
				Log.Add ("GetKey (int)");
				return base.GetKey (index);
			}

			public override string[] GetValues (int index)
			{
				Log.Add ("GetValues (int)");
				return base.GetValues (index);
			}

			public override string[] GetValues (string name)
			{
				Log.Add ("GetValues (string)");
				return base.GetValues (name);
			}

			public override void Add (string name, string value)
			{
				Log.Add ("Add (string, string)");
				base.Add (name, value);
			}

			public override void Set (string name, string value)
			{
				Log.Add ("Set (string, string)");
				base.Set (name, value);
			}
		}
	}
}
