// Web service test for WSDL document:
// http://localhost:8080/DataService.asmx?wsdl

using System;
using System.Data;
using NUnit.Framework;
using DataServiceTests.Soap;
//using DataServiceTests.HttpGet;
//using DataServiceTests.HttpPost;

namespace Localhost.DataServiceTests
{
	[TestFixture]
	public class DataServiceTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			DataService service = new DataService ();
			DataSet dset = service.QueryData ("some query");
			DataTable t = dset.Tables["PhoneNumbers"];
			
			AssertNotNull ("#1", t);
			AssertEquals ("#2", 2, t.Rows.Count);
			
			DataRow row = t.Rows[0];
			AssertEquals ("#3", "Lluis", row["name"]);
			AssertEquals ("#4", "23452345", row["home"]);
			row = t.Rows[1];
			AssertEquals ("#5", "Pep", row["name"]);
			AssertEquals ("#6", "435345", row["home"]);
			
			DataRow newRow = t.NewRow();
			newRow["name"] = "Pau";
			newRow["home"] = "9028374";
			t.Rows.Add (newRow);
			int n = service.SaveData (dset);
			AssertEquals ("#7", 3, n);
		}
	}
}
