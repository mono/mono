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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

using nwind;

// test ns Linq_101_Samples
#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP.Linq_101_Samples
#elif ORACLE
    namespace Test_NUnit_Oracle.Linq_101_Samples
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#elif FIREBIRD
    namespace Test_NUnit_Firebird.Linq_101_Samples
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737930.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class String_Date_functions : TestBase
    {
        [Test(Description = "String Concatenation. This sample uses the & operator to concatenate string fields and string literals in forming the Customers' calculated Location value")]
        public void LinqToSqlString01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select new { c.CustomerID, Location = c.City + ", " + c.Country };

            //bool foundBerlin = false;
            foreach (var v in q)
            {
                if (v.Location == "Berlin, Germany")
                {
                    //foundBerlin = true;
                    return;
                }
            }
            Assert.Fail("Expected to find location 'Berlin, Germany'");
        }

        [Test(Description = "String.Length. This sample uses the Length property to find all Products whose name is shorter than 10 characters.")]
        public void LinqToSqlString02()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.ProductName.Length < 10
                    select p;

            List<Product> prods = q.ToList();
            Assert.IsTrue(prods.Count > 0, "Expected some products");
        }

        [Test(Description = "String.Contains(substring).This sample uses the Contains method to find all Customers whose contact name contains 'Anders'.")]
        public void LinqToSqlString03()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.ContactName.Contains("Anders")
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.IndexOf(substring). This sample uses the IndexOf method to find the first instance of a space in each Customer's contact name.")]
        public void LinqToSqlString04()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select new { c.ContactName, SpacePos = c.ContactName.IndexOf(" ") };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.StartsWith(prefix). This sample uses the StartsWith method to find Customers whose contact name starts with 'Maria'.")]
        public void LinqToSqlString05()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.ContactName.StartsWith("Maria")
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.EndsWith(suffix). This sample uses the StartsWith method to find Customers whose contact name ends with 'Anders'.")]
        public void LinqToSqlString06()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.ContactName.EndsWith("Anders")
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Substring(start). This sample uses the Substring method to return Product names starting from the fourth letter.")]
        public void LinqToSqlString07()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    select p.ProductName.Substring(3);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Substring(start, length). This sample uses the Substring method to find Employees whose home phone numbers have '555' as the seventh through ninth digits.")]
        public void LinqToSqlString08()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HomePhone.Substring(5, 3) == "555"
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.ToUpper(). This sample uses the ToUpper method to return Employee names where the last name has been converted to uppercase.")]
        public void LinqToSqlString09()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select new { LastName = e.LastName.ToUpper(), e.FirstName };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test(Description = "String.ToLower(). This sample uses the ToLower method to return Category names that have been converted to lowercase.")]
        public void LinqToSqlString10()
        {
            Northwind db = CreateDB();

            var q = from c in db.Categories
                    select c.CategoryName.ToLower();

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Trim(). This sample uses the Trim method to return the first five digits of Employee home phone numbers, with leading and trailing spaces removed.")]
        public void LinqToSqlString11()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where !("  "+(e.LastName)).Trim().Contains(" ")
                    select e.LastName;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Insert(pos, str). This sample uses the Insert method to return a sequence of employee phone numbers that have a ) in the fifth position, inserting a : after the ).")]
        public void LinqToSqlString12()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HomePhone.Substring(4, 1) == ")"
                    select e.HomePhone.Insert(5, ":");

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Remove(start). This sample uses the Insert method to return a sequence of employee phone numbers that have a ) in the fifth position, removing all characters starting from the tenth character.")]
        public void LinqToSqlString13()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HomePhone.Substring(4, 1) == ")"
                    select e.HomePhone.Remove(9);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Remove(start, length). This sample uses the Insert method to return a sequence of employee phone numbers that have a ) in the fifth position, removing the first six characters.")]
        public void LinqToSqlString14()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HomePhone.Substring(4, 1) == ")"
                    select e.HomePhone.Remove(0, 6);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "String.Replace(find, replace). This sample uses the Replace method to return a sequence of Supplier information where the Country field has had UK replaced with United Kingdom and USA replaced with United States of America.")]
        public void LinqToSqlString15()
        {
            Northwind db = CreateDB();

            var q = from s in db.Suppliers
                    select new { s.CompanyName, Country = s.Country.Replace("UK", "United Kingdom").Replace("USA", "United States of America") };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test(Description = "DateTime.Year. This sample uses the DateTime's Year property to find Orders placed in 1997.")]
        public void LinqToSqlString16()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Year == 1996
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test(Description = "DateTime.Month. This sample uses the DateTime's Month property to find Orders placed in December.")]
        public void LinqToSqlString17()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Month == 10
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test(Description = "DateTime.Day. This sample uses the DateTime's Day property to find Orders placed on the 31st day of the month.")]
        public void LinqToSqlString18()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Day == 16
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test(Description = "DateTime.Seconds. This sample uses the DateTime's Day property to find Orders placed on the 31st day of the month.")]
        public void LinqToSqlString19()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                    where o.OrderDate.Value.Second == 16
                    select o).ToList();

 
        }
    }
}
