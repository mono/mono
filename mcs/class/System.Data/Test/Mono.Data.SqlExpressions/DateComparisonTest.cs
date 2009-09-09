using System;
using System.Data;
using NUnit.Framework;

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
		public void TestDateComparisonLeft()
		{
			DataView dataView = new DataView(dataSet.Tables["thing"], "start = #2009-07-19 00:01:00#", "", DataViewRowState.CurrentRows);
			Assert.AreEqual (1, dataView.Count);
		}
	}
}
