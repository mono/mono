//
// System.Data.Common.DbProviderFactoriesConfigurationHandlerTest.cs
//
// Author:
//   Sureshkumar T (tsureshkumar@novell.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOBILE && !XAMMAC_4_5

using System.IO;
using System.Xml;
using System.Globalization;
using System.Configuration;

using System.Data;
using System.Data.Common;

using NUnit.Framework;

namespace MonoTests.System.Data.Common 
{
	[TestFixture]
	public class DbProviderFactoriesConfigurationHandlerTest
	{
		const string configSection = "system.data_test";

		[Test]
		public void GetConfigTest ()
		{
			object o = ConfigurationSettings.GetConfig (configSection);
			DataSet ds = o as DataSet;
			Assert.IsNotNull (ds, "#A1");
			Assert.AreEqual ("system.data", ds.DataSetName, "#A2");
			Assert.AreEqual (1, ds.Tables.Count, "#A3");

			DataTable dt = ds.Tables [0];
			Assert.AreEqual ("DbProviderFactories", dt.TableName, "#B1");
			Assert.AreEqual (4, dt.Columns.Count, "#B2");
			Assert.IsNotNull (dt.Columns ["Name"], "#B3");
			Assert.IsNotNull (dt.Columns ["Description"], "#B4");
			Assert.IsNotNull (dt.Columns ["InvariantName"], "#B5");
			Assert.IsNotNull (dt.Columns ["AssemblyQualifiedName"], "#B6");

			DataColumn [] pk = dt.PrimaryKey;
			Assert.AreEqual (1, pk.Length, "#C1");
			Assert.AreEqual ("InvariantName", pk [0].ColumnName, "#C2");
		}

		[Test]
		public void PopulateTest ()
		{
			object o = ConfigurationSettings.GetConfig (configSection);
			DataSet ds = o as DataSet;
			DataTable dt = ds.Tables [0];
			Assert.AreEqual (2, dt.Rows.Count, "#A1");

			DataRow r = dt.Rows.Find ("ProviderTest.InvariantName");
			Assert.AreEqual ("ProviderTest.Name", r ["Name"].ToString (), "#B2");
			Assert.AreEqual ("ProviderTest.Description", r ["Description"].ToString (), "#B3");
			Assert.AreEqual ("ProviderTest.InvariantName", r ["InvariantName"].ToString (), "#B4");
			Assert.AreEqual ("ProviderTest.AssemblyQualifiedName", r ["AssemblyQualifiedName"].ToString (), "#B5");

			r = dt.Rows.Find ("ProviderTest4.InvariantName");
			Assert.AreEqual ("ProviderTest4.Name", r ["Name"].ToString (), "#A2");
			Assert.AreEqual ("ProviderTest4.Description", r ["Description"].ToString (), "#A3");
			Assert.AreEqual ("ProviderTest4.InvariantName", r ["InvariantName"].ToString (), "#A4");
			Assert.AreEqual ("ProviderTest4.AssemblyQualifiedName", r ["AssemblyQualifiedName"].ToString (), "#A5");
		}

		[Test]
		[Category ("NotWorking")]
		public void PopulateTest_Machine ()
		{
			object o = ConfigurationSettings.GetConfig ("system.data");
			DataSet ds = o as DataSet;
			DataTable dt = ds.Tables ["DbProviderFactories"];
			Assert.IsNotNull (dt, "#B1");
			Assert.IsTrue (dt.Rows.Count > 1, "#B2");
			DataRow r = dt.Rows.Find ("ProviderTest2.InvariantName");
			Assert.AreEqual ("ProviderTest2.Name", r ["Name"].ToString (), "#B3");
			Assert.AreEqual ("ProviderTest2.Description", r ["Description"].ToString (), "#B4");
			Assert.AreEqual ("ProviderTest2.InvariantName", r ["InvariantName"].ToString (), "#B5");
			Assert.AreEqual ("ProviderTest2.AssemblyQualifiedName", r ["AssemblyQualifiedName"].ToString (), "#B6");
		}

		[Test]
		public void PopulateFactoriesTest () // bug #80894
		{
			DataTable dt = DbProviderFactories.GetFactoryClasses ();
		}
	}
}

#endif
