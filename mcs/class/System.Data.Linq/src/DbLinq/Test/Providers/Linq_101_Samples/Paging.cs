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
    class CustomerComparer : IEqualityComparer<Customer>
    {
        public bool Equals(Customer x, Customer y)
        {
            if (object.ReferenceEquals (x, y))
                return true;
            return x.Address == y.Address &&
                x.City == y.City &&
                x.CompanyName == y.CompanyName &&
                x.ContactName == y.ContactName &&
                x.ContactTitle == y.ContactTitle &&
                x.Country == y.Country &&
                x.CustomerID == y.CustomerID &&
                x.Fax == y.Fax &&
                x.Phone == y.Phone &&
                x.PostalCode == y.PostalCode &&
                x.Region == y.Region &&
                true; // TODO: compare Orders: x.Orders.SequenceEqual(y.Orders, new OrderComparer());
        }

        public int GetHashCode(Customer obj)
        {
 	        throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class Paging : TestBase
    {
        [Test(Description = "Paging - Index. This sample uses the Skip and Take operators to do paging by skipping the first 50 records and then returning the next 10, thereby providing the data for page 6 of the Products table.")]
        public void LinqToSqlPaging01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     orderby c.ContactName
                     select c)
                    .Skip(1)
                    .Take(2);

            var expected = new[]{
                new Customer {
                    Address       = "Heerstr. 22",
                    City          = "Leipzig",
                    CompanyName   = "Morgenstern Gesundkost",
                    ContactName   = "Alexander Feuer",
                    ContactTitle  = "Marketing Assistant",
                    Country       = "Germany",
                    CustomerID    = "MORGK",
                    Fax           = null,
                    Phone         = "0342-023176",
                    PostalCode    = "04179",
                    Region        = null
                },
                new Customer {
                    Address       = "Avda. de la Constitución 2222",
                    City          = "México D.F.",
                    CompanyName   = "Ana Trujillo Emparedados y helados",
                    ContactName   = "Ana Trujillo",
                    ContactTitle  = "Owner",
                    Country       = "Mexico",
                    CustomerID    = "ANATR",
                    Fax           = "(5) 555-3745",
                    Phone         = "(5) 555-4729",
                    PostalCode    = "05021",
                    Region        = null
                },
            };
// The ordering of space characters depends on collation so
// lets jst check if the query worked on PostgreSQL.
#if !POSTGRES
            Assert.IsTrue(expected.SequenceEqual(q, new CustomerComparer()));
#else
            Assert.IsTrue(q.ToList().Count == 2);
#endif
        }

        [Test(Description = "Paging - Ordered Unique Key. This sample uses a where clause and the take operator to do paging by, first filtering to get only the ProductIDs above 50 (the last ProductID from page 5), then ordering by ProductID, and finally taking the first 10 results, thereby providing the data for page 6 of the Products table. Note that this method only works when ordering by a unique key.")]
        public void LinqToSqlPaging02()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products
                     where p.ProductID > 3
                     orderby p.ProductID
                     select p)
                    .Take(10);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Paging - Index. This sample uses the Skip and Take operators to do paging by skipping the first 50 records and then returning the next 10, thereby providing the data for page 6 of the Products table.")]
        public void LinqToSqlPaging03()
        {
            // This is basically LinqToSqlPaging01() without the `orderby` clause.
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select c)
                    .Skip(1)
                    .Take(2);

            var expected = new[]{
                new Customer {
                    Address       = "Avda. de la Constitución 2222",
                    City          = "México D.F.",
                    CompanyName   = "Ana Trujillo Emparedados y helados",
                    ContactName   = "Ana Trujillo",
                    ContactTitle  = "Owner",
                    Country       = "Mexico",
                    CustomerID    = "ANATR",
                    Fax           = "(5) 555-3745",
                    Phone         = "(5) 555-4729",
                    PostalCode    = "05021",
                    Region        = null
                },
                new Customer {
                    Address       = "Mataderos  2312",
                    City          = "México D.F.",
                    CompanyName   = "Antonio Moreno Taquería",
                    ContactName   = "Antonio Moreno",
                    ContactTitle  = "Owner",
                    Country       = "Mexico",
                    CustomerID    = "ANTON",
                    Fax           = null,
                    Phone         = "(5) 555-3932",
                    PostalCode    = "05023",
                    Region        = null
                },
            };
// The ordering of space characters depends on collation so
// lets jst check if the query worked on PostgreSQL.
#if !POSTGRES
            Assert.IsTrue(expected.SequenceEqual(q, new CustomerComparer()));
#else
            Assert.IsTrue(q.ToList().Count == 2);
#endif
        }
    }
}
