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
#elif MSSQL && MONO_STRICT
    namespace Test_NUnit_MsSql_Strict
#elif MSSQL
    namespace Test_NUnit_MsSql
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#endif
{
    [TestFixture]
    public class ReadTests_StringFunctions : TestBase
    {
        [Test]
        public void Insert01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.Insert(3, ":");


            var list = q.ToList();
            Assert.IsTrue(list.All(lastname => lastname.Contains(":")));
        }

        [Test]
        public void Insert02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.Insert(3, ":").Contains(":")
                    select e.LastName.Insert(3, ":");


            var list = q.ToList();
            Assert.IsTrue(list.All(lastname => lastname.Contains(":")));
        }

        [Test]
        public void Replace01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " .".Replace('.', 'a') == " a"
                    select e;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());

        }

        [Test]
        public void Replace02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.Replace('A', 'B').Contains("B")
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void Replace03()
        {
            //white-box test: Testing preevalutation of the where predicate (SpecialExpression.Execute method) before of building the sql query
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " .".Replace(" ", "f") == "f."
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Replace04()
        {
            //white-box test: Testing the select's projection field execution in clr.
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName + " .".Replace('.', 'a');

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void Replace05()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select " .".Replace(" ", "f");
            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void IndexOf01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " fu".IndexOf("fu") == 1
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void IndexOf02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.IndexOf("Fu") == 0
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void IndexOf03()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " fu".IndexOf('f') == 1
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void IndexOf04()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.IndexOf('F') == 0
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }



        [Test]
        public void IndexOf05()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.IndexOf("u", 1) == 1
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }



        [Test]
        public void IndexOf06()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.IndexOf('u', 1, 1) == 1
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void IndexOf08()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.IndexOf("u", 1, 1) == 1
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        [Test]
        public void IndexOf09()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.IndexOf("Fu") == 0;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void IndexOf10()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select " fu".IndexOf('f') == 1;


            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void IndexOf11()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.IndexOf('F') == 0;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }



        [Test]
        public void IndexOf12()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.IndexOf("u", 1) == 1;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }



        [Test]
        public void IndexOf13()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.IndexOf('u', 1, 1) == 1;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void IndexOf14()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.IndexOf("u", 1, 1) == 1;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }


        [Test]
        public void Remove01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " .".Remove(1) == " "
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void Remove02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.Remove(1).Length > 0
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test]
        public void Remove03()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where " ..".Remove(1, 2) == " "
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Remove04()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.LastName.Remove(1, 2).Length > 0
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Remove05()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select " .".Remove(1) == " ";

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void Remove06()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.Remove(1).Length > 0;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());

        }

        [Test]
        public void Remove07()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select " ..".Remove(1, 2) == " ";

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void Remove08()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select e.LastName.Remove(1, 2).Length > 0;

            var list = q.ToList();
            Assert.AreEqual(list.Count, db.Employees.Count());
        }

        [Test]
        public void StartsWith01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.StartsWith("ALF")
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }

        [Test]
        public void StartsWith02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID == "ALFKI"
                    select c.CustomerID.StartsWith("ALF");

            bool matchStart = q.Single();
            Assert.IsTrue(matchStart);
        }

        [Test]
        public void EndsWith01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.EndsWith("LFKI")
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }

        [Test]
        public void EndsWith02()
        {
            string param = "LFKI";
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.EndsWith(param)
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }

        [Test]
        public void EndsWith03()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where "ALFKI".EndsWith("LFKI")
                    select c.CustomerID;

            string custID = q.Single();
            Assert.IsTrue(custID == "ALFKI");
        }

        [Test]
        public void EndsWith04()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select c.CustomerID.EndsWith("LFKI");

            Assert.IsTrue(q.Any(r => r == true));
        }

        [Test]
        public void StartsWithPercent01()
        {
            string param = "%";
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID.StartsWith(param)
                    select c.CustomerID;

            int cnt = q.Count();
            Assert.AreEqual(0, cnt);
        }

        [Test]
        public void LTrim01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where !((e.LastName)).TrimStart().Contains(" ")
                    select e.LastName;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void LTrim02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select !((e.LastName)).TrimStart().Contains(" ");

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void RTrim01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where !((e.LastName)).TrimEnd().Contains(" ")
                    select e.LastName;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void RTrim02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select !((e.LastName)).TrimEnd().Contains(" ");

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Substring01()
        {
            Northwind db = CreateDB();

            int index = 0;
            var q = (from e in db.Customers
                     where e.CustomerID == "WARTH"
                     select new { name = e.CustomerID.Substring(index) }).First();

            Assert.AreEqual(q.name, "WARTH".Substring(index));
        }

        [Test]
        public void Substring02()
        {
            Northwind db = CreateDB();

            var q = (from e in db.Customers
                     where e.CustomerID.Substring(2) == "RTH"
                     select e);

            Assert.IsTrue(q.Any());
        }

        [Test]
        public void Substring03()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HomePhone.Substring(4, 1) == ")"
                    select new { A = e.HomePhone.Remove(0, 6), B = e.HomePhone.Substring(4, 1) };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

            var Employees = db.Employees.ToArray();

            var q2 = (from e in Employees
                      where e.HomePhone != null && e.HomePhone.Substring(4, 1) == ")"
                      select new { A = e.HomePhone.Remove(0, 6), B = e.HomePhone.Substring(4, 1) }).ToArray();

            Assert.AreEqual(list.Count, q2.Count());

            for (int i = 0; i < list.Count; i++)
                Assert.AreEqual(list[i], q2[i]);

        }

    }
}
