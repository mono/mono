using System;
using System.Data;
using NUnit.Framework;

namespace Monotests_Mono.Data.SqlExpressions
{
	[TestFixture]	
	public class DataColumnExprTest
	{
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
