// Web service test for WSDL document:
// http://www.forta.com/cf/tips/syndicate.cfc?wsdl

using System;
using NUnit.Framework;
using syndicateTests.Soap;

namespace syndicateTests
{
	[TestFixture]
	public class syndicateTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			syndicateService ss = new syndicateService ();
			Map map = ss.Get (new DateTime(2004,1,1));
			AssertNotNull ("map", map);
			AssertNotNull ("map.item", map.item);
			AssertEquals ("map.item.Length", 11, map.item.Length);
			AssertEquals ("PREV", map.item[0].key);
			AssertEquals ("true", map.item[0].value);
			AssertEquals ("TITLE", map.item[2].key);
			AssertEquals ("Ben Forta's ColdFusion Tip-of-the-Day", map.item[2].value);
			AssertEquals ("HEADER", map.item[8].key);
			AssertEquals ("Faster Dreamweaver Loading", map.item[8].value);
			
			QueryBean qb = ss.Search ("dreamweaver");
			AssertNotNull ("qb", qb);
			AssertNotNull ("qb", qb.columnList);
			AssertEquals ("qb.columnList.Length", 3, qb.columnList.Length);
			AssertEquals ("qb.columnList[0]", "TITLE", qb.columnList[0]);
			AssertEquals ("qb.columnList[1]", "AGE", qb.columnList[1]);
			AssertEquals ("qb.columnList[2]", "DATE", qb.columnList[2]);
			
			AssertNotNull ("qb.data", qb.data);
			AssertNotNull ("qb.data[0]", qb.data[0]);
			AssertEquals ("qb.data.Length", 38, qb.data.Length);
			AssertEquals ("qb.data[12].Length", 3, qb.data[12].Length);
			AssertEquals ("qb.data[12][0]", "Diagnosing FTP Problems", qb.data[12][0]);
			AssertEquals ("qb.data[12][1]", 528, qb.data[12][1]);
			AssertEquals ("qb.data[12][2]", new DateTime(2002, 8, 16), qb.data[12][2]);
		}
	}
}
