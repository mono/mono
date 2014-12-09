using System;
using System.Data;
#if WINDOWS_STORE_APP
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
using AssertionException = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.UnitTestAssertException;
#else
using NUnit.Framework;
#endif

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
