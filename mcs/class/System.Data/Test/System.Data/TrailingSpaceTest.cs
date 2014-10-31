using NUnit.Framework;
using System;
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class ComparisonTest {

		[Test]
		public void TestStringTrailingSpaceHandling () {
			// test for bug 79695 - does not ignore certain trailing whitespace chars when comparing strings
			DataTable dataTable = new DataTable ("Person");
			dataTable.Columns.Add ("Name", typeof (string));
			dataTable.Rows.Add (new object[] {"Mike   "}); 
			DataRow[] selectedRows = dataTable.Select ("Name = 'Mike'");
			Assert.AreEqual (1, selectedRows.Length, "Trailing whitespace chars not ignored");
		}
	}
}
