using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [TestFixture]
    public class ExecuteQuery_Test : TestBase
    {
        [Test]
        public void X1_SimpleQuery()
        {
            var db = CreateDB();

            IList<Category> categories1 = (from c in db.Categories orderby c.CategoryName select c).ToList();
            IList<Category> categories2 = db.ExecuteQuery<Category>(
                @"select 
                        [Description], 
                        [CategoryName], 
                        [Picture],
                        [CategoryID]
                    from [Categories]
                     order by [CategoryName]").ToList();

            Assert.AreEqual(categories1.Count, categories2.Count);
            for (int index = 0; index < categories2.Count; index++)
            {
                Assert.AreEqual(categories1[index].CategoryID, categories2[index].CategoryID);
                Assert.AreEqual(categories1[index].CategoryName, categories2[index].CategoryName);
                Assert.AreEqual(categories1[index].Description, categories2[index].Description);
            }
        }

        [Test]
        public void X2_CheckChanges()
        {
            var db = CreateDB();
            string query = "SELECT * FROM \"Customers\";";

            var characters = db.ExecuteQuery<Customer>(query);
            var character = characters.First();

            string beforecountry = character.Country;
            character.Country = "Burmuda";

            Assert.Greater(db.GetChangeSet().Updates.Count, 0);
            db.SubmitChanges();

            var character2 = db.Customers.First(c=>c.CustomerID==character.CustomerID);
            Assert.AreEqual(character2.Country, "Burmuda");

            character2.Country = beforecountry;
            db.SubmitChanges();
        }
    }
}
