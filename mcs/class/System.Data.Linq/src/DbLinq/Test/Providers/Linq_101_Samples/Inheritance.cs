using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;

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
    [TestFixture]
    public class Inheritance : TestBase
    {
        [Linq101SamplesModified("Original code did a reference to a newdb nortwhind that didn't exist, currently here uses db instead. Besides Contact type didn't exist")]
        [Test(Description = "Simple. This sample returns all contacts where the city is London.")]
        public void LinqToSqlInheritance01()
        {
            Northwind db = CreateDB();

            Assert.Ignore();

            //var cons = from c in db.Contacts
            //           select c;

            //var list = cons.ToList();
            //Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Original code did a reference to a newdb nortwhind that didn't exist, currently here uses db instead. Besides Contact type didn't exist")]
        [Test(Description = "OfType. This sample uses OfType to return all customer contacts.")]
        public void LinqToSqlInheritance02()
        {
            Northwind db = CreateDB();

            Assert.Ignore();

            //var cons = from c in newDB.Contacts.OfType<CustomerContact>()
            //           select c;

            //var list = cons.ToList();
            //Assert.IsTrue(list.Count > 0);

        }

        [Linq101SamplesModified("This test could not be implemented since FullContact is not defined.")]
        [Test(Description = "CType. This sample uses CType to return FullContact or null.")]
        public void LinqToSqlInheritance04()
        {
            Northwind db = CreateDB();

            Assert.Ignore();

            //var cons = from c in newDB.Contacts
            //           select (FullContact)c;

            //var list = cons.ToList();
            //Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("This test could not be implemented since CustomerContact is not defined.")]
        [Test(Description = "Cast. This sample uses a cast to retrieve customer contacts who live in London.")]
        public void LinqToSqlInheritance05()
        {
            Northwind db = CreateDB();

            Assert.Ignore();

            //var cons = from c in newDB.Contacts
            //           where c.ContactType == "Customer" && (CustomerContact)c.City == "London"
            //           select c;

            //var list = cons.ToList();
            //Assert.IsTrue(list.Count > 0);


        }

        [Linq101SamplesModified("Original code did a reference to a newdb nortwhind that didn't exist, currently here uses db instead. Besides Contact type didn't exist")]
        [Test(Description = "UseAsDefault. This sample demonstrates that an unknown contact type will be automatically converted to the default contact type.")]
        public void LinqToSqlInheritance06()
        {
            Northwind db = CreateDB();

            Assert.Ignore();

            //var contact = new Contact() { ContactType = null, CompanyName = "Unknown Company", City = "London", Phone = "333-444-5555" };
            //db.Contacts.Add(contact);
            //db.SubmitChanges();

            //var con = (from c in db.Contacts
            //           where c.ContactType == null
            //           select c).First();

        
        }

        [Linq101SamplesModified("Original code did a reference to a newdb nortwhind that didn't exist, currently here uses db instead. Besides Contact type didn't exist")]
        [Test(Description = "Insert New Record. This sample demonstrates how to create a new shipper contact.")]
        public void LinqToSqlInheritance07()
        {
            Northwind db = CreateDB();

            Assert.Ignore();

            //var ShipperContacts = from sc in newDB.Contacts.OfType<ShipperContact>()
            //                      where sc.CompanyName = "Northwind Shipper"
            //                      select sc;


            //var nsc = new ShipperContact() { CompanyName = "Northwind Shipper", Phone = "(123)-456-7890" };
            //db.Contacts.Add(nsc);
            //db.SubmitChanges();


            //ShipperContacts = from sc in db.Contacts.OfType<ShipperContact>()
            //                  where sc.CompanyName == "Northwind Shipper"
            //                  select sc;


            //newDB.Contacts.Remove(nsc);
            //newDB.SubmitChanges();
        }
    }
}
