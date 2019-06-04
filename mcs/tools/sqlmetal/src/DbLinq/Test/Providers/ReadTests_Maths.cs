using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;

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
    public class ReadTests_Maths : TestBase
    {

        [Test]
        public void Abs01()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Abs((double)c.ProductID) > 0.0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Abs02()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Abs((double)(c.Quantity)) > 0.0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        //Devuelve el valor absoluto (positivo) de una expresión numérica.

        //EXP (SSIS)

#if !DEBUG && (SQLITE && MONO)
        [Explicit]
#endif
        [Test]
        public void Exp()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Exp((double)(c.Quantity)) > 0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        //Devuelve el exponente de la base e de la expresión especificada.

        //CEILING (SSIS)


        //Devuelve el menor entero mayor o igual que una expresión numérica.

#if !DEBUG && (SQLITE && MONO)
        [Explicit]
#endif
        [Test]
        public void Floor()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Floor((double)(c.Discount)) > 0
                    select c;

            var list = q.ToList();

        }


        //Devuelve el mayor entero que es menor o igual que una expresión numérica.

        //LN (SSIS)

#if !DEBUG && (SQLITE)
        [Explicit]
#endif
        [Test]
        public void Log01()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Log((double)(c.Discount + 1)) > 0.0
                    select c;

            Assert.AreEqual(838, q.Count());
        }

#if !DEBUG && (SQLITE || POSTGRES)
        [Explicit]
#endif
        [Test]
        public void Log02()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Log((double)(c.Discount + 1),3.0) > 0.0
                    select c;

            Assert.AreEqual(838, q.Count());
        }


        //Devuelve el logaritmo natural de una expresión numérica.

        //LOG (SSIS)
#if !DEBUG && (SQLITE && MONO)
        [Explicit]
#endif
        [Test]
        public void Log03()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Log10((double)(c.Discount + 1)) > 0.0
                    select c;

            Assert.AreEqual(838, q.Count());
        }


        //Devuelve el logaritmo en base 10 de una expresión numérica.

        //POWER (SSIS)

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void Pow()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Pow((double)(c.Discount), 2.0) > 0
                    select c;

            var list = q.ToList();

        }
        //Devuelve el resultado de elevar una expresión numérica a una determinada potencia.

        //ROUND (SSIS)
#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        public void Round()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Round((double)(c.Discount), MidpointRounding.AwayFromZero) > 0
                    select c;

            var list = q.ToList();

        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Round02()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Round((double)(c.Discount)) > 0
                    select c;

            var list = q.ToList();
        }

        //Devuelve una expresión numérica, redondeada a la longitud o precisión especificada. .

        //SIGN (SSIS)


#if !DEBUG && (SQLITE && MONO)
        [Explicit]
#endif
        [Test]
        public void Sign01()
        {
            Northwind db = CreateDB();

            var q = from c in db.OrderDetails
                    where Math.Sign((double)(c.Discount)) > 0d
                    select c;

            var list = q.ToList();
        }
        //Devuelve el signo positivo (+), cero (0) o negativo (-) de una expresión numérica.

        //SQUARE (SSIS)


        //Devuelve el cuadrado de una expresión numérica.

        //SQRT (SSIS) 

#if !DEBUG && (SQLITE && MONO)
        [Explicit]
#endif
        [Test]
        public void Sqrt()
        {
            Northwind db = CreateDB();
            //Employee e;
            //Order o;
            var q = from c in db.OrderDetails
                    where Math.Sqrt((double)(c.Discount)) > 0
                    select c;

            var list = q.ToList();
        }
    }

}
