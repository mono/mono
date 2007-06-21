// OdbcConnectionStringBuilderTest.cs - NUnit Test Cases for testing the
// OdbcConnectionStringBuilder Class.
//
// Authors:
//      Nidhi Rawal (rawalnidhi_rawal@yahoo.com)
// 
// Copyright (c) 2007 Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#if NET_2_0
using System;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using Mono.Data;

using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcConnectionStringBuilderTest

	{
		[Test]
		public void IndexerTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Dbq"] = "C:\\Data.xls";
			builder ["DriverID"] = "790";
			builder ["DefaultDir"] = "C:\\";	
			Assert.AreEqual ("790", builder ["DriverID"], "#1 The value of the key DriverID is not as expected");
			Assert.AreEqual ("C:\\Data.xls", builder ["Dbq"], "#2 The value of the key Dbq is not as expected");
			Assert.AreEqual ("C:\\", builder ["DefaultDir"], "#3 The value of the key DefaultDir is not as expected");
		}

		[Test]
		public void ConnectionStringConstructorTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ("Driver={SQL Server};Server=(local);Database=AdventureWorks;Uid=ab;Pwd=pass@word1");
			Assert.AreEqual ("AdventureWorks", builder ["Database"],"#1 The value of the key AdventureWorks does not match with the one set in the constructor");
			Assert.AreEqual ("pass@word1", builder ["Pwd"], "#2 The value of the key Pwd does not match with the one set in the constructor");
			Assert.AreEqual ("ab", builder ["Uid"], "#3 The value of the key Uid does not match with the one set in the constructor");
			Assert.AreEqual ("{SQL Server}", builder ["Driver"], "#4 The value of the key Driver does not match with the one set in the constructor");
			Assert.AreEqual ("(local)", builder ["Server"],"#5 The value of the key Server does not match with the one set in the constructor");
			OdbcConnectionStringBuilder oDriver = new OdbcConnectionStringBuilder ("Driver=");
			Assert.AreEqual ("", oDriver.ConnectionString, "#6 It should not add keyword without value");
			OdbcConnectionStringBuilder oDsn = new OdbcConnectionStringBuilder ("Dsn=");
			Assert.AreEqual ("", oDsn.ConnectionString, "#6 It should not add keyword without value");
		}	
		
		// Test case failing
		[Test]
		public void ConnectionStringTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder (@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=|DataDirectory|\Northwind.mdb;Jet OLEDB:System Database=|DataDirectory|\System.mdw;"); 
			Assert.AreEqual ("Microsoft.Jet.OLEDB.4.0", builder ["Provider"], "#1 ");
			Assert.AreEqual (@"|DataDirectory|\Northwind.mdb", builder ["Data Source"], "#2 ");
			Assert.AreEqual (@"|DataDirectory|\System.mdw", builder ["Jet OLEDB:System Database"], "#3 ");

			OdbcConnectionStringBuilder builder1 = new OdbcConnectionStringBuilder (); 
			builder1 ["Data Source"] = "(local)";
			builder1 ["Integrated Security"] = true;
			builder1 ["Initial Catalog"] = "AdventureWorks;NewValue=Bad";
			Assert.AreEqual ("Integrated Security=True;Initial Catalog={AdventureWorks;NewValue=Bad};Data Source=(local)",
					 builder1.ConnectionString, "#4 "); // Not in same sequence as MS.NET
			
			OdbcConnectionStringBuilder builder2 = new OdbcConnectionStringBuilder (@"Driver={Microsoft Excel Driver (*.xls)};DBQ=c:\bin\book1.xls"); 
			Assert.AreEqual ("{Microsoft Excel Driver (*.xls)}", builder2["Driver"], "#5 ");
			Assert.AreEqual (@"c:\bin\book1.xls", builder2["DBQ"], "#6 ");
			
			OdbcConnectionStringBuilder builder3 = new OdbcConnectionStringBuilder (@"Driver={Microsoft Text Driver (*.txt; *.csv)};DBQ=c:\bin");
			Assert.AreEqual ("{Microsoft Text Driver (*.txt; *.csv)}", builder3["Driver"], "#7 "); // Not giving correct value
			Assert.AreEqual (@"c:\bin", builder3["DBQ"], "#8 ");
		}
		
		[Test]
		public void ConnectionStringConstructorNullTest ()

		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder (null);
			Assert.AreEqual ("", builder.ConnectionString, "#1"); 
			OdbcConnectionStringBuilder oc = new OdbcConnectionStringBuilder ("");
			Assert.AreEqual ("", oc.ConnectionString, "#2");
		}
		
		[Test]
		public void DuplicateKeyTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["DriverID"] = "120";
			builder ["DriverID"] = "790";
			Assert.AreEqual ("DriverID=790", builder.ConnectionString, "#1 The connection string should take most recent value");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IndexerValueNullTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["DriverID"] = null;
			Assert.AreEqual (null, builder ["DriverID"], "#1");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void IndexerKeywordNullTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder [null] = "abc";
			Assert.AreEqual ("abc", builder [null], "#1");
		}
		
		[Test]
		public void DriverTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Driver = "SQL Server";
			Assert.AreEqual ("Driver={SQL Server}", builder.ConnectionString, "#1 The connection string should contain the value for the driver that is being set by the property");
			Assert.AreEqual ("SQL Server", builder.Driver, "#2 The property should return the value that is being set");
			builder.Clear ();
			builder ["Driver"] = "SQL Server";
			Assert.AreEqual ("Driver={SQL Server}", builder.ConnectionString, "#3 The connection string should contain the value for the driver that is being set by assigning the value to key");
			Assert.AreEqual ("SQL Server", builder.Driver, "#4 The property should return the value that is being set by assigning the value to the key");			
			builder.Clear ();
			builder ["Driver"] = "{SQL Server";
        		Assert.AreEqual ("Driver={{SQL Server}", builder.ConnectionString, "#5 ");
        		Assert.AreEqual ("{SQL Server", builder.Driver, "#6 ");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DriverNullTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Driver = null;
		}	
		[Test]
		public void IndexerDriverNullTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Driver"] = null;
			Assert.AreEqual ("", builder.Driver, "#1 ");
			Assert.AreEqual ("", builder ["Driver"], "#2 ");
		}	

		[Test]
		public void DsnTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Dsn = "myDsn";
			Assert.AreEqual ("Dsn=myDsn", builder.ConnectionString, "#1 The connection string should contain the value for the dsn that is being set by the property");
			Assert.AreEqual ("myDsn", builder.Dsn, "#2 The property should return the value that is being set");
			builder.Clear ();
			builder ["Dsn"] = "myDsn";
			Assert.AreEqual ("Dsn=myDsn", builder.ConnectionString, "#3 The connection string should contain the value for the dsn that is being set by assigning the value to key");
			Assert.AreEqual ("myDsn", builder.Dsn, "#4 The property should return the value that is being set by assigning the value to the key");			
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DsnNullTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Dsn = null;
		}

		[Test]
		public void IndexerDsnNullTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Dsn"] = null;
			Assert.AreEqual ("", builder.Dsn, "#1 ");
			Assert.AreEqual ("", builder ["Dsn"], "#1 ");
		}	
		
		
		[Test]
		public void ClearTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["Dbq"] = "C:\\Data.xls";
			builder ["DriverID"] = "790";
			builder ["DefaultDir"] = "C:\\";
			builder.Clear ();
			Assert.AreEqual ("", builder.ConnectionString, "#1 The connection string should be null");
			Assert.AreEqual (false, builder.ContainsKey ("Dbq"), "#2 ");
		}

		[Test]
		public void ContainsKeyTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["SourceType"] = "DBC";
			Assert.AreEqual (true, builder.ContainsKey ("SourceType"), "#1 Should be true for explicitly added key");
			Assert.AreEqual (true, builder.ContainsKey ("Dsn"), "#2 Should return true for the key that is implicitly added");
			Assert.AreEqual (true, builder.ContainsKey ("Driver"), "#3 Should return true for the key that is implicitly added");
			Assert.AreEqual (false, builder.ContainsKey ("xyz"), "#4 Should return false for the non-existant key");
			builder.Dsn = "myDsn";
			Assert.AreEqual (true, builder.ContainsKey ("Dsn"), "#5 Should return true as the key Dsn is now explicitly added");
			builder.Driver = "SQL Server";
			Assert.AreEqual (true, builder.ContainsKey ("Driver"), "#6 Should return true as the key Driver is now explicitly added");
			builder ["Dsn"] = "myDsn";
			Assert.AreEqual (true, builder.ContainsKey ("Dsn"), "#5 Should return true as the key Dsn is now explicitly added");
			builder ["Driver"] = "SQL Server";
			Assert.AreEqual (true, builder.ContainsKey ("Driver"), "#6 Should return true as the key Driver is now explicitly added");
			builder ["abc"] = "pqr";
			Assert.AreEqual (true, builder.ContainsKey ("ABC"), "#7 Should return true as it should not be case sensitive");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ContainsKeyNullArgumentTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["SourceType"] = "DBC";
			builder.ContainsKey (null);
		}

		[Test]
		public void RemoveTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder ["DriverID"] = "790";
			builder ["DefaultDir"] = "C:\\";
			Assert.AreEqual (false, builder.Remove ("Dsn"), "#1 Should return false for the key that is not explicitly added");
			Assert.AreEqual (false, builder.Remove ("Driver"), "#2 Should return false for the key that is not explicitly added");
			Assert.AreEqual (true, builder.Remove ("DriverID"), "#3 It should remove the explicitly added key and return true");
			Assert.AreEqual (false, builder.Remove ("userid"), "#4 It should return false as there is no such key");
			Assert.AreEqual (false, builder.Remove ("DriverID"), "#5 It should not find the key that is previously removed and should return false");
			builder.Dsn = "myDsn";
			Assert.AreEqual (true, builder.Remove ("Dsn"), "#6 Should return true as the key Dsn is now explicitly added");			
			builder.Driver = "SQL Server";
			Assert.AreEqual (true, builder.Remove ("Driver"), "#7 Should return true as the key Driver is now explicitly added");
			builder ["Dsn"] = "myDsn";
			Assert.AreEqual (true, builder.Remove ("Dsn"), "#8 Should return true as the key Dsn is now explicitly added");			
			Assert.AreEqual (false, builder.Remove ("Dsn"), "#9 Should return false as the key is already removed");			
			builder ["Driver"] = "SQL Server";
			Assert.AreEqual (true, builder.Remove ("Driver"), "#10 Should return true as the key Driver is now explicitly added");
			Assert.AreEqual (false, builder.Remove ("Driver"), "#11 Should return false as the key is already removed");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNullArgumentTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Remove (null);
		}

		[Test]
		public void TryGetValueTest ()

		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			object value = null;

			builder ["DriverID"] = "790";
			builder ["Server"] = "C:\\";
			Assert.AreEqual (true, builder.TryGetValue ("DriverID", out value), "#1 It should find the key and return true");
			Assert.AreEqual (true, builder.TryGetValue ("SERVER", out value), "#2 It should return true as it is not case-sensitive");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryGetValueNullArgumentTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			object value = null;
			builder.TryGetValue (null, out value);
		}
		
		[Test]
		public void AddTest ()
		{
			OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder ();
			builder.Add ("driverid", "420");
			builder.Add ("DriverID", "840");
			Assert.AreEqual ("840", builder ["driverid"], "#1 it should overrite the previous value of driverid as its case-insensitive");
			builder.Add ("Driver", "OdbcDriver");
			Assert.AreEqual ("OdbcDriver", builder.Driver, "#2 The value of driver should be as per the added value");
			builder.Add ("Driver", "{OdbcDriver");
			Assert.AreEqual ("{OdbcDriver", builder.Driver, "#3 The value of driver should be as per the added value");	
			builder.Add ("Dsn", "MyDsn");
			Assert.AreEqual ("MyDsn", builder.Dsn, "#4 The value of dsn should be as per the added value");
		}
	}
}
#endif // NET_2_0 using

