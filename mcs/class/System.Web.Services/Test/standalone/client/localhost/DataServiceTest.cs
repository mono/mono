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
			
			Assert.IsNotNull (t, "#1");
			Assert.AreEqual (2, t.Rows.Count, "#2");
			
			DataRow row = t.Rows[0];
			Assert.AreEqual ("Lluis", row["name"], "#3");
			Assert.AreEqual ("23452345", row["home"], "#4");
			row = t.Rows[1];
			Assert.AreEqual ("Pep", row["name"], "#5");
			Assert.AreEqual ("435345", row["home"], "#6");
			
			DataRow newRow = t.NewRow();
			newRow["name"] = "Pau";
			newRow["home"] = "9028374";
			t.Rows.Add (newRow);
			int n = service.SaveData (dset);
			Assert.AreEqual (3, n, "#7");
		}
	}
}
