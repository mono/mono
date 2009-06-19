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
using System.Globalization;
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
    /// <summary>
    /// this test class will exercise various operands, such as 'a&&b', 'a>=b', ""+a, etc.
    /// </summary>
    [TestFixture]
    public class ReadTest_Operands : TestBase
    {

        [Test]
        public void H1_SelectConcat()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products select p.ProductName + " " + p.SupplierID;
            int count = 0;
            foreach (string s in q)
            {
                if (s == null)
                    continue; //concat('X',NULL) -> NULL 

                bool ok = Char.IsLetterOrDigit(s[0]) && s.Contains(' ');
                Assert.IsTrue(ok, "Concat string should start with product name, instead got:" + s);
                count++;
            }
            Assert.IsTrue(count > 0, "Expected concat strings, got none");
        }

        [Test]
        public void H2_SelectGreaterOrEqual()
        {
            Northwind db = CreateDB();

            var q = db.Products.Where(p => p.ProductID >= 5);
            int count = 0;
            foreach (Product p in q)
            {
                Assert.IsTrue(p.ProductID >= 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>=5, got none");
        }

        public struct ProductWrapper1
        {
            public int ProductID { get; set; }
            public int? SupplierID { get; set; }
        }

        public class ProductWrapper2
        {
            public int ProductID { get; set; }
            public int? SupplierID { get; set; }
        }

        public class ProductWrapper3
        {
            public int ProductID { get; set; }
            public int? SupplierID { get; set; }
            public ProductWrapper3(int p, int? s) { ProductID = p; SupplierID = s; }
            public ProductWrapper3(int p, int? s, bool unused) { ProductID = p; SupplierID = s; }
        }

        [Test]
        public void H3_Select_MemberInit_Struct()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 5
                    select new ProductWrapper1 { ProductID = (int)p.ProductID, SupplierID = (int?)p.SupplierID };
            int count = 0;
            foreach (ProductWrapper1 p in q)
            {
                Assert.IsTrue(p.ProductID > 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>5, got none");
        }

        [Test]
        public void H4_Select_MemberInit_Class()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 5
                    select new ProductWrapper2 { ProductID = (int)p.ProductID, SupplierID = (int?)p.SupplierID };
            int count = 0;
            foreach (ProductWrapper2 p in q)
            {
                Assert.IsTrue(p.ProductID > 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>5, got none");
        }

        [Test]
        public void H5_Select_MemberInit_Class2()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID > 5
                    select new ProductWrapper3((int)p.ProductID, (int?)p.SupplierID);
            int count = 0;
            foreach (ProductWrapper3 p in q)
            {
                Assert.IsTrue(p.ProductID > 5, "Failed on ProductID>=20");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID>5, got none");
        }

        [Test]
        public void H6_SelectNotEqual()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products
                    where p.ProductID != 1
                    select p;
            int count = 0;
            foreach (Product p in q)
            {
                Assert.IsFalse(p.ProductID == 1, "Failed on ProductID != 1");
                count++;
            }
            Assert.IsTrue(count > 0, "Expected some products with ProductID != 1, got none");
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void J1_LocalFunction_DateTime_ParseExact()
        {
            Northwind db = CreateDB();

            //Lookup EmployeeID 1:
            //Andy Fuller - HireDate: 1989-01-01 00:00:00

            string hireDate = "1992.08.14";

            // Ingres assumes UTC on all date queries
            var q = from e in db.Employees
#if INGRES
                    where e.HireDate == DateTime.ParseExact(hireDate, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
#else
                    where e.HireDate == DateTime.ParseExact(hireDate, "yyyy.MM.dd", CultureInfo.InvariantCulture)
#endif
                    select e.LastName;
            var empLastName = q.Single(); //MTable_Projected.GetQueryText()
            Assert.AreEqual("Fuller", empLastName);
        }

    }
}
