#region MIT license
// 
// MIT license
//
// Copyright (c) 2009 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Linq;

using Mono.Data.Sqlite;

using nwind;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using NUnit.Framework;

namespace Test_NUnit_Sqlite
{
    [TestFixture]
    public class DirectDataContextTest
    {
        [Test]
        public void CreateDataContext()
        {
            string connectionString = "DbLinqProvider=Sqlite;" + 
                "DbLinqConnectionType=Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite;" + 
                "Data Source=Northwind.db3";
            var dc = new DataContext(connectionString);
            Assert.AreEqual(typeof(SqliteConnection), dc.Connection.GetType());

            var dcq = from p in dc.GetTable<Product>() where p.ProductName == "Chai" select p.ProductID;
            var cmd = dc.GetCommand(dcq);
            var dcc = dcq.ToList().Count;
            Assert.AreEqual(dcc, 1);
        }
    }
}