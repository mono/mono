using System;
using System.Data;
#if USE_MSUNITTEST
#if WINDOWS_PHONE || NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixtureAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestCategoryAttribute;
#else // !WINDOWS_PHONE && !NETFX_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using CategoryAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCategoryAttribute;
#endif // WINDOWS_PHONE || NETFX_CORE
#else // !USE_MSUNITTEST
using NUnit.Framework;
#endif // USE_MSUNITTEST

namespace Monotests_Mono.Data.SqlExpressions
{
	[TestFixture]	
	public class DateComparisonTest
	{
		private DataSet dataSet;

		[SetUp]
		public void SetUp()
		{
			dataSet = new DataSet();
			dataSet.ReadXml("Test/Mono.Data.SqlExpressions/dateComparisonTest.xml", XmlReadMode.InferSchema);
		}

		[Test]
		public void TestDateComparisonRight()
		{
			DataView dataView = new DataView(dataSet.Tables["thing"], "#2009-07-19 00:01:00# = start", "", DataViewRowState.CurrentRows);
			Assert.AreEqual (1, dataView.Count);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void TestDateComparisonLeft()
		{
			DataView dataView = new DataView(dataSet.Tables["thing"], "start = #2009-07-19 00:01:00#", "", DataViewRowState.CurrentRows);
			Assert.AreEqual (1, dataView.Count);
		}
	}
}
