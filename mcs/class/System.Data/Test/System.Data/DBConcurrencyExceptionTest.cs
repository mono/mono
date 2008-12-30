//
// DBConcurrencyExceptionTest.cs - NUnit Test Cases for DBConcurrencyException
//
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2008 Gert Driesen
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

using System;
using System.Data;

using NUnit.Framework;

namespace MonoTests_System.Data
{
	[TestFixture]
	public class DBConcurrencyExceptionTest
	{
		[Test] // .ctor ()
		public void Constructor1 ()
		{
			DBConcurrencyException dbce = new DBConcurrencyException ();
			Assert.IsNull (dbce.InnerException, "InnerException");
			Assert.IsNotNull (dbce.Message, "Message1");
#if NET_2_0
			Assert.IsNotNull (dbce.Message, "Message2:" + dbce.Message);
#else
			Assert.AreEqual (new SystemException ().Message, dbce.Message, "Message2:" + dbce.Message);
#endif
			Assert.IsNull (dbce.Row, "Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "RowCount");
#endif
		}

		[Test] // .ctor (String)
		public void Constructor2 ()
		{
			DBConcurrencyException dbce;
			string msg = "MONO";

			dbce = new DBConcurrencyException (msg);
			Assert.IsNull (dbce.InnerException, "#A:InnerException");
			Assert.AreSame (msg, dbce.Message, "#A:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#A:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#A:RowCount");
#endif

			dbce = new DBConcurrencyException ((string) null);
			Assert.IsNull (dbce.InnerException, "#B:InnerException");
			Assert.IsNotNull (dbce.Message, "#B:Message1");
			Assert.IsTrue (dbce.Message.IndexOf (typeof (DBConcurrencyException).FullName) != -1, "#B:Message2:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#B:Row");

#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#B:RowCount");
#endif

			dbce = new DBConcurrencyException (string.Empty);
			Assert.IsNull (dbce.InnerException, "#C:InnerException");
			Assert.AreEqual (string.Empty, dbce.Message, "#C:Message");
			Assert.IsNull (dbce.Row, "#C:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#C:RowCount");
#endif
		}

		[Test] // .ctor (String, Exception)
		public void Constructor3 ()
		{
			Exception inner = new Exception ();
			DBConcurrencyException dbce;
			string msg = "MONO";

			dbce = new DBConcurrencyException (msg, inner);
			Assert.AreSame (inner, dbce.InnerException, "#A:InnerException");
			Assert.AreSame (msg, dbce.Message, "#A:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#A:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#A:RowCount");
#endif

			dbce = new DBConcurrencyException ((string) null, inner);
			Assert.AreSame (inner, dbce.InnerException, "#B:InnerException");
			Assert.IsTrue (dbce.Message.IndexOf (typeof (DBConcurrencyException).FullName) != -1, "#B:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#B:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#B:RowCount");
#endif

			dbce = new DBConcurrencyException (string.Empty, inner);
			Assert.AreSame (inner, dbce.InnerException, "#C:InnerException");
			Assert.AreEqual (string.Empty, dbce.Message, "#C:Message");
			Assert.IsNull (dbce.Row, "#C:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#C:RowCount");
#endif

			dbce = new DBConcurrencyException (msg, (Exception) null);
			Assert.IsNull (dbce.InnerException, "#D:InnerException");
			Assert.AreSame (msg, dbce.Message, "#D:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#D:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#D:RowCount");
#endif

			dbce = new DBConcurrencyException ((string) null, (Exception) null);
			Assert.IsNull (dbce.InnerException, "#E:InnerException");
			Assert.IsTrue (dbce.Message.IndexOf (typeof (DBConcurrencyException).FullName) != -1, "#E:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#E:Row");
#if NET_2_0
			Assert.AreEqual (0, dbce.RowCount, "#E:RowCount");
#endif
		}

#if NET_2_0
		[Test] // .ctor (String, Exception, DataRow [])
		public void Constructor4 ()
		{
			DataTable dt = new DataTable ();
			DataRow rowA = dt.NewRow ();
			DataRow rowB = dt.NewRow ();
			DataRow [] rows;
			Exception inner = new Exception ();
			DBConcurrencyException dbce;
			string msg = "MONO";

			rows = new DataRow [] { rowA, null, rowB };
			dbce = new DBConcurrencyException (msg, inner, rows);
			Assert.AreSame (inner, dbce.InnerException, "#A:InnerException");
			Assert.AreSame (msg, dbce.Message, "#A:Message:" + dbce.Message);
			Assert.AreSame (rowA, dbce.Row, "#A:Row");
			Assert.AreEqual (3, dbce.RowCount, "#A:RowCount");

			rows = new DataRow [] { rowB, rowA, null };
			dbce = new DBConcurrencyException ((string) null, inner, rows);
			Assert.AreSame (inner, dbce.InnerException, "#B:InnerException");
			Assert.IsTrue (dbce.Message.IndexOf (typeof (DBConcurrencyException).FullName) != -1, "#B:Message:" + dbce.Message);
			Assert.AreSame (rowB, dbce.Row, "#B:Row");
			Assert.AreEqual (3, dbce.RowCount, "#B:RowCount");

			rows = new DataRow [] { null, rowA };
			dbce = new DBConcurrencyException (string.Empty, inner, rows);
			Assert.AreSame (inner, dbce.InnerException, "#C:InnerException");
			Assert.AreEqual (string.Empty, dbce.Message, "#C:Message");
			Assert.IsNull (dbce.Row, "#C:Row");
			Assert.AreEqual (2, dbce.RowCount, "#C:RowCount");

			rows = new DataRow [] { rowA };
			dbce = new DBConcurrencyException (msg, (Exception) null, rows);
			Assert.IsNull (dbce.InnerException, "#D:InnerException");
			Assert.AreSame (msg, dbce.Message, "#D:Message:" + dbce.Message);
			Assert.AreSame (rowA, dbce.Row, "#D:Row");
			Assert.AreEqual (1, dbce.RowCount, "#D:RowCount");

			rows = null;
			dbce = new DBConcurrencyException (msg, (Exception) null, rows);
			Assert.IsNull (dbce.InnerException, "#E:InnerException");
			Assert.AreSame (msg, dbce.Message, "#E:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#E:Row");
			Assert.AreEqual (0, dbce.RowCount, "#E:RowCount");

			rows = null;
			dbce = new DBConcurrencyException ((string) null, (Exception) null, rows);
			Assert.IsNull (dbce.InnerException, "#F:InnerException");
			Assert.IsTrue (dbce.Message.IndexOf (typeof (DBConcurrencyException).FullName) != -1, "#F:Message:" + dbce.Message);
			Assert.IsNull (dbce.Row, "#F:Row");
			Assert.AreEqual (0, dbce.RowCount, "#F:RowCount");
		}
#endif

		[Test]
		public void Row ()
		{
			DataTable dt = new DataTable ();
			DataRow rowA = dt.NewRow ();
			DataRow rowB = dt.NewRow ();

			DBConcurrencyException dbce = new DBConcurrencyException ();
			dbce.Row = rowA;
			Assert.AreSame (rowA, dbce.Row, "#A:Row");
#if NET_2_0
			Assert.AreEqual (1, dbce.RowCount, "#A:RowCount");
#endif
			dbce.Row = rowB;
			Assert.AreSame (rowB, dbce.Row, "#B:Row");
#if NET_2_0
			Assert.AreEqual (1, dbce.RowCount, "#B:RowCount");
#endif
			dbce.Row = null;
			Assert.IsNull (dbce.Row, "#C:Row");
#if NET_2_0
			Assert.AreEqual (1, dbce.RowCount, "#C:RowCount");
#endif
		}
	}
}
