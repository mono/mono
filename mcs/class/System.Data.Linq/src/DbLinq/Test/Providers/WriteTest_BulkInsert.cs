#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Test_NUnit;

using nwind;

// test ns 
#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP
#elif ORACLE
    namespace Test_NUnit_Oracle
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict
#elif MSSQL
    namespace Test_NUnit_MsSql
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#endif
{
#if !MONO_STRICT
    [TestFixture]
    public class WriteTest_BulkInsert : TestBase
    {
#if !DEBUG && (SQLITE || MSSQL)
        [Explicit]
#endif
        [Test]
        public void BI01_InsertProducts()
        {
            int initialCount = 0, countAfterBulkInsert = 0;

            Northwind db = CreateDB();
            initialCount = db.Products.Count();

            //DbLinq.vendor.mysql.MySqlVendor.UseBulkInsert[db.Products] = 3; //insert three rows at a time
            // picrap: inject this information in the IVendor (and check this is necessary)

            db.Products.BulkInsert(new[]
                                       {
                                           NewProduct("tmp_ProductA"),
                                           NewProduct("tmp_ProductB"),
                                           NewProduct("tmp_ProductC"),
                                           NewProduct("tmp_ProductD")
                                       });
            db.SubmitChanges();

            //confirm that we indeed inserted four rows:
            Northwind db2 = CreateDB();
            countAfterBulkInsert = db2.Products.Count();
            Assert.IsTrue(countAfterBulkInsert == initialCount + 4);

            //clean up
            base.ExecuteNonQuery("DELETE FROM Products WHERE ProductName LIKE 'tmp_%'");
        }
    }
#endif
}
