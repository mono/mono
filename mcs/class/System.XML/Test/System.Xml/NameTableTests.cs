//
// System.Xml.NameTableTests.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// Author: Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc.
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class NameTableTests : Assertion
	{
		NameTable table;
		
		[SetUp]
		public void GetReady ()
		{
			table = new NameTable ();
		}

		//
		// Tests System.Xml.NameTable.Add (string)
		//		
		[Test]
		public void Add1 ()
		{
			string add = "add1";
			string testAdd = table.Add (add);
			AssertEquals ("#1", add, testAdd);
			AssertSame ("#2", add, testAdd);

			testAdd = table.Add ("");
			AssertEquals ("#3", string.Empty, testAdd);
			AssertSame ("#4", string.Empty, testAdd);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Add1_Null ()
		{
			table.Add ((string) null);
		}

		//
		// Tests System.Xml.NameTable.Add (char[], int, int)
		//		
		[Test]
		public void Add2 ()
		{
			char[] test = new char [4] { 'a', 'd', 'd', '2' };
			int index = 0;
			int length = 3; // "add"

			string testAdd = table.Add (test, index, length);
			AssertEquals ("#1", "add", testAdd);

			testAdd = table.Add ((char[]) null, 0, 0);
			AssertEquals ("#2", string.Empty, testAdd);
			AssertSame ("#3", string.Empty, testAdd);

			testAdd = table.Add (new char[0], 0, 0);
			AssertEquals ("#4", string.Empty, testAdd);
			AssertSame ("#5", string.Empty, testAdd);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Add2_Null ()
		{
			table.Add ((char[]) null, 0, 1);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Add2_InvalidIndex ()
		{
			table.Add (new char[3] { 'a', 'b', 'c' }, 4, 1);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Add2_InvalidLength ()
		{
			table.Add (new char[0], 0, 1);
		}

		//
		// Tests System.Xml.NameTable.Get (string)
		//
		[Test]
		public void Get1 ()
		{
			string get1 = "get1";
			string testGet = table.Add (get1);
			AssertEquals ("#1", "get1", testGet);

			AssertEquals ("#2", testGet, table.Get (get1));
			AssertSame ("#3", get1, testGet );

			testGet = table.Get ("");
			AssertEquals ("#1", string.Empty, testGet);
			AssertSame ("#2", string.Empty, testGet);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Get1_Null ()
		{
			table.Get ((string) null);
		}

		//
		// Tests System.Xml.NameTable.Get (char[], int, int)
		//
		[Test]
		public void Get2 ()
		{
			char[] test = new char [4] { 'g', 'e', 't', '2' };
			int index = 0; 
			int length = 3; // "get"
			
			string testGet = table.Add (test, index, length);
			AssertEquals ("#1", "get", testGet);

			AssertEquals ("#2", testGet, table.Get ("get"));
			AssertEquals ("#3", testGet, table.Get (test, index, length));
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Get2_Null ()
		{
			table.Get ((char[]) null, 0, 1);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Get2_InvalidIndex ()
		{
			table.Get (new char[3] { 'a', 'b', 'c' }, 4, 1);
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void Get2_InvalidLength ()
		{
			table.Get (new char[3] { 'a', 'b', 'c' }, 2, 6);
		}

		//
		// Tests System.Xml.NameTable.Get (char[], int, 0)
		//
		[Test]
		public void Get3 ()
		{
			string testGet = null;

			testGet = table.Get ((char[]) null, 10, 0);
			AssertEquals ("#1", string.Empty, testGet);
			AssertSame ("#2", string.Empty, testGet);

			testGet = table.Get (new char[0], 2, 0);
			AssertEquals ("#3", string.Empty, testGet);
			AssertSame ("#4", string.Empty, testGet);

			testGet = table.Get (new char[3] { 'a', 'b', 'c' }, 5, 0);
			AssertEquals ("#5", string.Empty, testGet);
			AssertSame ("#6", string.Empty, testGet);
		}
	}
}
