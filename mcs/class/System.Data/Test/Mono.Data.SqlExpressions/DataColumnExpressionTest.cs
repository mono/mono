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
}
