// Collection.cs - NUnit Test Cases for Microsoft.VisualBasic.Collection
//
// Authors:
//   Chris J. Breisch (cjbreisch@altavista.net)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Chris J. Breisch
// (C) Martin Willemoes Hansen
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using Microsoft.VisualBasic;
using System.Collections;

namespace MonoTests.Microsoft.VisualBasic
{
	[TestFixture]
	public class CollectionTest
	{
		[SetUp]
		public void GetReady () {
		}

		[TearDown]
		public void Clean () {
		}

		// Test Constructor
		[Test]
		public void New ()
		{
			Collection c;

			c = new Collection ();

			Assert.IsNotNull (c, "#N01");
			Assert.AreEqual (0, c.Count, "#N02");
		}

		// Test Add method with Key == null
		[Test]
		public void AddNoKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add (typeof (int), null, null, null);
			c.Add (typeof (double), null, null, null);
			c.Add (typeof (string), null, null, null);

			Assert.AreEqual (3, c.Count, "#ANK01");

			// Collection class is 1-based
			Assert.AreEqual (typeof (string), c[3], "#ANK02");
		}

		// Test Add method with Key specified
		[Test]
		public void AddKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Football", "Foot", null, null);
			c.Add ("Basketball", "Basket", null, null);
			c.Add ("Volleyball", "Volley", null, null);

			Assert.AreEqual (4, c.Count, "#AK01");

			// Collection class is 1-based
			Assert.AreEqual ("Baseball", c[1], "#AK02");
			Assert.AreEqual ("Volleyball", c["Volley"], "#AK03");
		}

		// Test Add method with Before specified and Key == null
		[Test]
		public void AddBeforeNoKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add (typeof (int), null, null, null);
			c.Add (typeof (double), null, 1, null);
			c.Add (typeof (string), null, 2, null);
			c.Add (typeof (object), null, 2, null);

			Assert.AreEqual (4, c.Count, "#ABNK01");

			// Collection class is 1-based
			Assert.AreEqual (typeof (int), c[4], "#ABNK02");
			Assert.AreEqual (typeof (double), c[1], "#ABNK03");
			Assert.AreEqual (typeof (object), c[2], "#ABNK04");
		}

		// Test Add method with Before and Key
		[Test]
		public void AddBeforeKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Football", "Foot", 1, null);
			c.Add ("Basketball", "Basket", 1, null);
			c.Add ("Volleyball", "Volley", 3, null);

			Assert.AreEqual (4, c.Count, "#ABK01");
			Assert.AreEqual ("Basketball", c[1], "#ABK02");
			Assert.AreEqual ("Baseball", c[4], "#ABK03");
			Assert.AreEqual ("Volleyball", c["Volley"], "#ABK04");
			Assert.AreEqual ("Football", c["Foot"], "#ABK05");
		}

		// Test Add method with After specified and Key == null
		[Test]
		public void AddAfterNoKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add (typeof (int), null, null, 0);
			c.Add (typeof (double), null, null, 1);
			c.Add (typeof (string), null, null, 1);
			c.Add (typeof (object), null, null, 3);

			Assert.AreEqual (4, c.Count, "#AANK01");
			Assert.AreEqual (typeof (object), c[4], "#AANK02");
			Assert.AreEqual (typeof (int), c[1], "#AANK03");
			Assert.AreEqual (typeof (string), c[2], "#AANK04");
		}

		// Test Add method with After and Key
		[Test]
		public void AddAfterKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add ("Baseball", "Base", null, 0);
			c.Add ("Football", "Foot", null, 1);
			c.Add ("Basketball", "Basket", null, 1);
			c.Add ("Volleyball", "Volley", null, 2);

			Assert.AreEqual (4, c.Count, "#AAK01");
			Assert.AreEqual ("Baseball", c[1], "#AAK02");
			Assert.AreEqual ("Football", c[4], "#AAK03");
			Assert.AreEqual ("Basketball", c["Basket"], "#AAK04");
			Assert.AreEqual ("Volleyball", c["Volley"], "#AAK05");
		}

		// Test GetEnumerator method
		[Test]
		public void GetEnumerator ()
		{
			Collection c;
			IEnumerator e;
			object[] o = new object[4] {typeof(int), 
				typeof(double), typeof(string), typeof(object)};
			int i = 0;

			c = new Collection ();

			c.Add (typeof (int), null, null, null);
			c.Add (typeof (double), null, null, null);
			c.Add (typeof (string), null, null, null);
			c.Add (typeof (object), null, null, null);

			e = c.GetEnumerator ();

			Assert.IsNotNull (e, "#GE01");

			while (e.MoveNext ()) {
				Assert.AreEqual (o[i], e.Current, "#GE02." + i.ToString ());
				i++;
			}

			e.Reset ();
			e.MoveNext ();

			Assert.AreEqual (o[0], e.Current, "#GE03");
		}

		// Test GetEnumerator method again, this time using foreach
		[Test]
		public void Foreach ()
		{
			Collection c;
			object[] o = new object[4] {typeof(int), 
				typeof(double), typeof(string), typeof(object)};
			int i = 0;

			c = new Collection ();

			c.Add (typeof (int), null, null, null);
			c.Add (typeof (double), null, null, null);
			c.Add (typeof (string), null, null, null);
			c.Add (typeof (object), null, null, null);


			foreach (object item in c) {
				Assert.AreEqual (o[i], item, "#fe01." + i.ToString ());
				i++;
			}

		}

		// Test Remove method with Index
		[Test]
		public void RemoveNoKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add (typeof (int), null, null, null);
			c.Add (typeof (double), null, null, null);
			c.Add (typeof (string), null, null, null);
			c.Add (typeof (object), null, null, null);

			Assert.AreEqual (4, c.Count, "#RNK01");

			c.Remove (3);

			Assert.AreEqual (3, c.Count, "#RNK02");

			// Collection class is 1-based
			Assert.AreEqual (typeof (object), c[3], "#RNK03");

			c.Remove (1);

			Assert.AreEqual (2, c.Count, "#RNK04");
			Assert.AreEqual (typeof (double), c[1], "#RNK05");
			Assert.AreEqual (typeof (object), c[2], "#RNK06");

			c.Remove (2);

			Assert.AreEqual (1, c.Count, "#RNK07");
			Assert.AreEqual (typeof (double), c[1], "#RNK08");

			c.Remove (1);

			Assert.AreEqual (0, c.Count, "#RNK09");
		}

		// Test Remove method with Key
		[Test]
		public void RemoveKey ()
		{
			Collection c;

			c = new Collection ();

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Football", "Foot", null, null);
			c.Add ("Basketball", "Basket", null, null);
			c.Add ("Volleyball", "Volley", null, null);

			Assert.AreEqual (4, c.Count, "#RK01");

			c.Remove ("Foot");

			Assert.AreEqual (3, c.Count, "#RK02");
			Assert.AreEqual ("Basketball", c["Basket"], "#RK03");

			// Collection class is 1-based
			Assert.AreEqual ("Volleyball", c[3], "#RK04");

			c.Remove ("Base");

			Assert.AreEqual (2, c.Count, "#RK05");
			Assert.AreEqual ("Basketball", c[1], "#RK06");
			Assert.AreEqual ("Volleyball", c["Volley"], "#RK07");

			c.Remove (2);

			Assert.AreEqual (1, c.Count, "#RK08");
			Assert.AreEqual ("Basketball", c[1], "#RK09");
			Assert.AreEqual ("Basketball", c["Basket"], "#RK10");

			c.Remove (1);

			Assert.AreEqual (0, c.Count, "#RK11");
		}

		// Test all the Exceptions we're supposed to throw
		[Test]
		public void Exception ()
		{
			Collection c = new Collection ();

			try {
				// nothing in Collection yet
				object o = c[0];
				Assert.Fail ("#E02");
			} catch (IndexOutOfRangeException) {
			}

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Football", "Foot", null, null);
			c.Add ("Basketball", "Basket", null, null);
			c.Add ("Volleyball", "Volley", null, null);

			try {
				// only 4 elements
				object o = c[5];
				Assert.Fail ("#E04");
			} catch (IndexOutOfRangeException) {
			}

			try {
				// Collection class is 1-based
				object o = c[0];
				Assert.Fail ("#E06");
			} catch (IndexOutOfRangeException) {
			}

			try {
				// no member with Key == "Kick"
				object o = c["Kick"];
				Assert.Fail ("#E08");
			} catch (ArgumentException) {
				// FIXME
				// VB Language Reference says IndexOutOfRangeException 
				// here, but MS throws ArgumentException
			}

			try {
				// Even though Indexer is an object, really it's a string
				object o = c[typeof (int)];
				Assert.Fail ("#E10");
			} catch (ArgumentException) {
			}

			try {
				// can't specify both Before and After
				c.Add ("Kickball", "Kick", "Volley", "Foot");
				Assert.Fail ("#E12");
			} catch (ArgumentException) {
			}

			try {
				// Key "Foot" already exists
				c.Add ("Kickball", "Foot", null, null);
				Assert.Fail ("#E14");
			} catch (ArgumentException) {
			}

			try {
				// Even though Before is object, it's really a string
				c.Add ("Dodgeball", "Dodge", typeof (int), null);
				Assert.Fail ("#E16");
			} catch (InvalidCastException) {
			}

			try {
				// Even though After is object, it's really a string
				c.Add ("Wallyball", "Wally", null, typeof (int));
				Assert.Fail ("#E18");
			} catch (InvalidCastException) {
			}

			try {
				// have to pass a legitimate value to remove
				c.Remove (null);
				Assert.Fail ("#E20");
			} catch (ArgumentNullException) {
			}

			try {
				// no Key "Golf" exists
				c.Remove ("Golf");
				Assert.Fail ("#E22");
			} catch (ArgumentException) {
			}

			try {
				// no Index 10 exists
				c.Remove (10);
				Assert.Fail ("#E24");
			} catch (IndexOutOfRangeException) {
			}

			try {
				IEnumerator e = c.GetEnumerator ();

				// Must MoveNext before Current
				object item = e.Current;
#if NET_2_0
				Assert.IsNull (item, "#E25");
#else
				Assert.Fail ("#E26");
#endif
			} catch (IndexOutOfRangeException) {
#if NET_2_0
				Assert.Fail ("#E27");
#endif
			}

			try {
				IEnumerator e = c.GetEnumerator ();
				e.MoveNext ();

				c.Add ("Paintball", "Paint", null, null);

				// Can't MoveNext if Collection has been modified
				e.MoveNext ();

				// FIXME
				// On-line help says this should throw an error. MS doesn't.
			} catch (Exception) {
				Assert.Fail ("#E28");
			}

			try {
				IEnumerator e = c.GetEnumerator ();
				e.MoveNext ();

				c.Add ("Racketball", "Racket", null, null);

				// Can't Reset if Collection has been modified
				e.Reset ();

				// FIXME
				// On-line help says this should throw an error. MS doesn't.
			} catch (InvalidOperationException) {
				Assert.Fail ("#E30");
			}
		}

		[Test]
		public void IList_Remove ()
		{
			Collection c = new Collection ();
			IList list = (IList) c;

			list.Remove (null);

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Paintball", "Paint", null, null);

			Assert.AreEqual (2, c.Count, "#1");

			try {
				list.Contains (null);
				Assert.Fail ("#2");
			} catch (NullReferenceException) {
			}

			Assert.AreEqual (2, c.Count, "#3");

			list.Remove (c.GetType ());
			Assert.AreEqual (2, c.Count, "#4");

			list.Remove (1);
			Assert.AreEqual (2, c.Count, "#5");

			list.Remove ("Something");
			Assert.AreEqual (2, c.Count, "#6");

			list.Remove ("Baseball");
			Assert.AreEqual (1, c.Count, "#7");
			Assert.AreEqual ("Paintball", c[1], "#8");
		}

		[Test]
		public void IList_RemoveAt ()
		{
			Collection c = new Collection ();
			IList list = (IList) c;

			try {
				list.RemoveAt (0);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				list.RemoveAt (-1);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Paintball", "Paint", null, null);

			Assert.AreEqual (2, c.Count, "#3");
			Assert.AreEqual ("Baseball", list[0], "#4");

			list.RemoveAt (0);
			Assert.AreEqual (1, c.Count, "#5");
			Assert.AreEqual ("Paintball", list[0], "#6");

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Basketball", "Basket", null, null);

			Assert.AreEqual ("Paintball", list[0], "#7");
			Assert.AreEqual ("Baseball", list[1], "#8");
			Assert.AreEqual ("Basketball", list[2], "#9");

			try {
				list.RemoveAt (3);
				Assert.Fail ("#10");
			} catch (ArgumentOutOfRangeException) {
			}

			list.RemoveAt (-1);
			Assert.AreEqual (2, c.Count, "#11");
			Assert.AreEqual ("Baseball", list[0], "#12");
			Assert.AreEqual ("Basketball", list[1], "#13");
		}

		[Test]
		public void IList_IndexOf ()
		{
			Collection c = new Collection ();
			IList list = (IList) c;

			Assert.AreEqual (-1, list.IndexOf (null), "#1");

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Paintball", "Paint", null, null);
			c.Add (5, "6", null, null);

			try {
				list.IndexOf (null);
				Assert.Fail ("#2");
			} catch (NullReferenceException) {
			}

			Assert.AreEqual (0, list.IndexOf ("Baseball"), "#3");
			Assert.AreEqual (-1, list.IndexOf ("Base"), "#4");

			Assert.AreEqual (1, list.IndexOf ("Paintball"), "#5");
			Assert.AreEqual (-1, list.IndexOf ("Pain"), "#6");

			Assert.AreEqual (2, list.IndexOf (5), "#7");
			Assert.AreEqual (-1, list.IndexOf (6), "#8");

			Assert.AreEqual (-1, list.IndexOf ("Something"), "#9");
		}

		[Test]
		public void IList_Contains ()
		{
			Collection c = new Collection ();
			IList list = (IList) c;

			Assert.IsFalse (list.Contains (null), "#1");

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Paintball", "Paint", null, null);
			c.Add (5, "6", null, null);

			try {
				list.Contains (null);
				Assert.Fail ("#2");
			} catch (NullReferenceException) {
			}

			Assert.AreEqual (true, list.Contains ("Baseball"), "#3");
			Assert.AreEqual (false, list.Contains ("Base"), "#4");

			Assert.AreEqual (true, list.Contains ("Paintball"), "#5");
			Assert.AreEqual (false, list.Contains ("Paint"), "#6");

			Assert.AreEqual (true, list.Contains (5), "#7");
			Assert.AreEqual (false, list.Contains (6), "#8");

			Assert.AreEqual (false, list.Contains ("Something"), "#9");
		}

		[Test]
		public void IList_Indexer_Get ()
		{
			Collection c = new Collection ();
			IList list = (IList) c;

			try {
				object value = list[0];
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				object value = list[-1];
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Paintball", "Paint", null, null);
			c.Add (5, "6", null, null);

			Assert.AreEqual ("Baseball", list[0], "#3");
			Assert.AreEqual ("Paintball", list[1], "#4");
			Assert.AreEqual (5, list[2], "#5");

			try {
				object value = list[3];
				Assert.Fail ("#6");
			} catch (ArgumentOutOfRangeException) {
			}

			object val = list[-1];
			Assert.AreEqual ("Baseball", val, "#6");
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // setter is badly broken in MS.NET 1.x
#endif
		public void IList_Indexer_Set ()
		{
			Collection c = new Collection ();
			IList list = (IList) c;

			try {
				list[0] = "Baseball";
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				list[-1] = "Baseball";
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			c.Add ("Baseball", "Base", null, null);
			c.Add ("Paintball", "Paint", null, null);
			c.Add (5, "6", null, null);

			Assert.AreEqual (3, c.Count, "#3");

			list[0] = "Basketball";
			list[2] = "Six";

			Assert.AreEqual (3, c.Count, "#4");
			Assert.AreEqual ("Basketball", list[0], "#5");
			Assert.AreEqual ("Paintball", list[1], "#6");
			Assert.AreEqual ("Six", list[2], "#7");

			try {
				list[3] = "Baseball";
				Assert.Fail ("#8");
			} catch (ArgumentOutOfRangeException) {
			}

			list[-1] = "Whatever";
			Assert.AreEqual (3, c.Count, "#8");
			Assert.AreEqual ("Whatever", list[0], "#9");
			Assert.AreEqual ("Paintball", list[1], "#10");
			Assert.AreEqual ("Six", list[2], "#11");
		}
	}
}
