//
// System.Data.Common.DbProviderFactoriesConfigurationHandler.cs
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

#if NET_2_0

using System.IO;
using System.Xml;
using System.Globalization;
using System.Configuration;

using System.Data;
using System.Data.Common;

using NUnit.Framework;

namespace MonoTests.System.Data.Common {
    [TestFixture]
	public class DbProviderFactoriesConfigurationHandlerTest
	{
        const string configSection = "system.data_test";

        [Test]
        public void GetConfigTest ()
        {
            object o = ConfigurationSettings.GetConfig (configSection);
            Assert.IsTrue (o is DataSet, "GetConfig should return DataSet");
            DataSet ds = o as DataSet;
            DataTable dt = ds.Tables [0];
            Assert.IsNotNull (dt.Columns ["Name"], "Name column missing");
            Assert.IsNotNull (dt.Columns ["Description"], "Description column missing");
            Assert.IsNotNull (dt.Columns ["InvariantName"], "InvariantName column missing");
            Assert.IsNotNull (dt.Columns ["AssemblyQualifiedName"], "AssemblyQualifiedName column missing");
            Assert.IsNotNull (dt.Columns ["SupportedClasses"], "SupportedClasses column missing");

            DataColumn [] pk = dt.PrimaryKey;
            Assert.AreEqual (1, pk.Length, "primary key column not set");
            Assert.AreEqual ("InvariantName", pk [0].ColumnName, "InvariantName should be the primary key");

        }

        [Test]
        public void PopulateTest ()
        {
            object o = ConfigurationSettings.GetConfig (configSection);
            DataSet ds = o as DataSet;
            DataTable dt = ds.Tables [0];
            DataRow r = dt.Rows.Find ("ProviderTest.InvariantName");
            Assert.AreEqual ("ProviderTest.Name", r ["Name"].ToString (), "Name column missing");
            Assert.AreEqual ("ProviderTest.Description", r ["Description"].ToString (), "Description column missing");
            Assert.AreEqual ("ProviderTest.InvariantName", r ["InvariantName"].ToString (), "InvariantName column missing");
            Assert.AreEqual ("ProviderTest.AssemblyQualifiedName", r ["AssemblyQualifiedName"].ToString (), "AssemblyQualifiedName column missing");
            Assert.AreEqual (255, (int) r ["SupportedClasses"], "SupportedClasses column missing");

        }
    }
}

#endif // NET_2_0
