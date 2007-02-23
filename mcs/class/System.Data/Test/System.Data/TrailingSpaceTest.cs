using NUnit.Framework;
using System;
using System.Data;

namespace Monotests_Mono.Data.SqlExpressions
{
	[TestFixture]
	public class ComparisonTest {

		[Test]
		public void TestStringTrailingSpaceHandling () {
			// test for bug 79695 - does not ignore certain trailing whitespace chars when comparing strings
			System.Data.DataTable dataTable = new System.Data.DataTable ("Person");
			dataTable.Columns.Add ("Name", typeof (string));
			dataTable.Rows.Add (new object[] {"Mike   "}); 
			DataRow[] selectedRows = dataTable.Select ("Name = 'Mike'");
			Assert.AreEqual (1, selectedRows.Length, "Trailing whitespace chars not ignored");
		}
	}
}
