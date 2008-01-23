using System;
using System.Data;
using NUnit.Framework;

namespace Monotests_Mono.Data.SqlExpressions
{
	[TestFixture]	
	public class DataColumnExprTest
	{
		[Test]
		public void TestDataColumnExpr0SingleColumnValue ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Col_0.Value", Type.GetType ("System.Int32"));
			table.Columns.Add ("Col_1", Type.GetType ("System.Int32"));
			table.Columns.Add ("Result", Type.GetType ("System.Int32"), "IIF(Col_0.Value, Col_1 + 5, 0)");

			DataRow row = table.NewRow ();
			row ["Col_0.Value"] = 0;
			row ["Col_1"] = 10;

			table.Rows.Add (row);
			Assert.AreEqual (0, (int)table.Rows[0][2], "#1");
		}
		
		[Test]
		public void TestDataColumnLikeExpr ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("c1");
			dt.Rows.Add (new string [] { null });
			dt.Rows.Add (new string [] { "xax" });
			dt.Columns.Add ("c2", typeof (bool), "c1 LIKE '%a%'");
			Assert.IsFalse ((bool) dt.Rows [0] [1]);
			Assert.IsTrue ((bool) dt.Rows [1] [1]);
		}
		
		[Test]
		public void TestDataColumnExpr0Literal ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Col_0.Value", Type.GetType ("System.Int32"));
			table.Columns.Add ("Col_1", Type.GetType ("System.Int32"));
			table.Columns.Add ("Result", Type.GetType ("System.Int32"), "IIF(false, Col_1 + 5, 0)");

			DataRow row = table.NewRow ();
			row ["Col_0.Value"] = 0;
			row ["Col_1"] = 10;

			table.Rows.Add (row);
			Assert.AreEqual (0, (int)table.Rows[0][2], "#1");
		}
		[Test]
		public void TestDataColumnExpr1 ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Col_0.Value", Type.GetType ("System.Int32"));
			table.Columns.Add ("Col_1", Type.GetType ("System.Int32"));
			table.Columns.Add ("Result", Type.GetType ("System.Int32"), "IIF(Col_0.Value > 10, Col_1 + 5, 0)");

			DataRow row = table.NewRow ();
			row ["Col_0.Value"] = 20;
			row ["Col_1"] = 10;

			table.Rows.Add (row);
			Assert.AreEqual ((int)table.Rows[0][1] + 5, table.Rows[0][2], "#1");
		}
		[Test]
		public void TestDataColumnExpr2 ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Col_0.Value", Type.GetType ("System.Int32"));
			table.Columns.Add ("Col_1", Type.GetType ("System.Int32"));
			table.Columns.Add ("Result", Type.GetType ("System.Int32"), "IIF(Col_0.Value > 10, Col_1 + 5, 0)");

			DataRow row = table.NewRow ();
			row ["Col_0.Value"] = 9;
			row ["Col_1"] = 10;

			table.Rows.Add (row);
			Assert.AreEqual (0, (int)table.Rows[0][2], "#1");
		}
		[Test]
		public void TestDataColumnSubstring ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Col_0", Type.GetType ("System.String"));
			table.Columns.Add ("Result", Type.GetType ("System.String"), "SUBSTRING(Col_0+'K?', 2+2, 2)");

			DataRow row = table.NewRow ();
			row ["Col_0"] = "Is O";

			table.Rows.Add (row);
			Assert.AreEqual ("OK", (string)table.Rows[0][1], "#1");
		}
		[Test]
		public void TestConcat ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Result", Type.GetType ("System.Int32"), "'3' + '2'");

			DataRow row = table.NewRow ();

			table.Rows.Add (row);
			Assert.AreEqual (32, table.Rows[0][0], "#1");
		}
		
		[Test]
		public void TestIsNull ()
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Result", typeof(bool), "('3') IS NULL");

			DataRow row = table.NewRow ();

			table.Rows.Add (row);
			Assert.AreEqual (false, table.Rows[0][0], "#1");
		}
	}
	[TestFixture]
	public class DataColumnCharTest
	{
		private static DataTable _dt = new DataTable();

		[Test]
		public void Test1 ()
		{
			_dt.Columns.Add(new DataColumn("a", typeof(char)));

			AddData('1');
			AddData('2');
			AddData('3');
			AddData('A');

			Assert.AreEqual (true, FindRow("'A'"), "Test1-1 failed");
			Assert.AreEqual (true, FindRow("65"), "Test1-2 failed");
			Assert.AreEqual (true, FindRow("'1'"), "Test1-3 failed");
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void Test2 ()
		{
			FindRow("'65'");
		}
		[Test]
		public void Test3 ()
		{
			Assert.AreEqual (false, FindRow ("1"), "Test3-1 failed");
		}

		private static bool FindRow(string f)
		{
			string filter = string.Format("a = {0}", f);

			DataRow[] rows = _dt.Select(filter);

			if (rows.Length == 0)
				return false;
			else
				return true;
		}

		private static void AddData(char a)
		{
			DataRow row = _dt.NewRow();
			row["a"] = a;
			_dt.Rows.Add(row);
		}
	}
}
