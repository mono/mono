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
#define USE_STORED_PROCS

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using nwind;
using System.Data.SQLite;

//using nwind;  // contains Northwind context

#if ORACLE
#if ODP
using xint = System.Int32;
using XSqlConnection = Oracle.DataAccess.Client.OracleConnection;
using XSqlCommand = Oracle.DataAccess.Client.OracleCommand;
#else
using xint = System.Int32;
using XSqlConnection = System.Data.OracleClient.OracleConnection;
using XSqlCommand = System.Data.OracleClient.OracleCommand;
#endif
#elif POSTGRES
using xint = System.Int32;
using XSqlConnection = Npgsql.NpgsqlConnection;
using XSqlCommand = Npgsql.NpgsqlCommand;
#elif SQLITE
using System.Data.SQLite;
using XSqlConnection = System.Data.SQLite.SQLiteConnection;
using XSqlCommand = System.Data.SQLite.SQLiteCommand;
#elif MSSQL
using XSqlConnection = System.Data.SqlClient.SqlConnection;
using XSqlCommand = System.Data.SqlClient.SqlCommand;
using xint = System.UInt32;
#else
using XSqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using XSqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using xint = System.UInt32;
#endif

namespace DbLinq.SQLite.Example
{
    class Program
    {
        static void Main(string[] args)
        {
#if SQLITE
            string connStr = "Data Source=Northwind.db3;FailIfMissing=false;";
            if (args.Length >= 1)
            {
                System.Data.SQLite.SQLiteConnection cnn = new SQLiteConnection(connStr);
                System.Data.SQLite.SQLiteCommand cmd = cnn.CreateCommand();
                for (int i = 0; i < args.Length; i++)
                {
                    cmd.CommandText = System.IO.File.ReadAllText(args[i]);
                    cnn.Open();
                    cmd.ExecuteNonQuery();
                    cnn.Close();
                }
                cmd.Dispose();
                cnn.Dispose();
            }
#else
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: DbLinq.MySql.Example.exe server user password database");
                Console.WriteLine("Debug arguments can be set on project properties in visual studio.");
                Console.WriteLine("Press enter to continue.");
                Console.ReadLine();
                return;
            }
            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}", args);
#endif

#if false
            SQLiteCommand cmd = new SQLiteCommand("select hello(?s)", new SQLiteConnection(connStr));
            //cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("?s", "xx");
            cmd.Parameters[0].Direction = ParameterDirection.Input; //.Value = "xx";
            cmd.Connection.Open();
            //SQLiteDataReader dr = cmd.ExecuteReader();
            object obj = cmd.ExecuteScalar();
#endif
            // BUG: contexts must to be disposable
            Northwind db = new Northwind(new XSqlConnection(connStr));

#if !SQLITE && USE_STORED_PROCS
            int is2;
            object xx = db.sp_selOrders("ZZ", out is2);
            string reply0 = db.hello0();
            string reply1 = db.Hello1("Pigafetta");
#endif
#if NO

            Console.Clear();
            Console.WriteLine("from at in db.Alltypes select at;");
            var q1 = from at in db.Alltypes select at;
            foreach (var v in q1)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
#endif

            Console.Clear();
            Console.WriteLine("from p in db.Products orderby p.ProductName select p;");
            var q2 = from p in db.Products orderby p.ProductName select p;
            foreach (var v in q2)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

            // BUG: This one throws a null reference for some reason.
            //Console.Clear();
            //var q3 = from c in db.Customers
            //         from o in c.Orders 
            //        where c.City == "London" select new { c, o };
            //foreach (var v in q3)
            //    ObjectDumper.Write(v);
            //Console.ReadLine();

            Console.Clear();
            Console.WriteLine("from p in db.Products where p.ProductID == 7 select p;");
            var q4 = from p in db.Products where p.ProductID == 7 select p;
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

#if !SQLITE
            Console.Clear();
            Console.WriteLine("from c in db.Customers from o in c.Orders where c.City == \"London\" select new { c, o };");
            var q5 = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
#endif

#if !SQLITE
            Console.Clear();
            Console.WriteLine("from o in db.Orders where o.Customer.City == \"London\" select new { c = o.Customer, o };");
            var q6 = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
#endif

            Console.Clear();
            Console.WriteLine("db.Orders");
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

#if !SQLITE
            // BUG: This currently will insert 3 rows when it should insert only 2
            // SubmitChanges isn't clearing the client side transaction data
            Console.Clear();
            Console.WriteLine("db.Orders.Add(new Order { ProductID = 7, CustomerID = 1, OrderDate = DateTime.Now });");            
            db.Orders.Add(new Order { EmployeeID = 1, CustomerID = "ALFKI", OrderDate = DateTime.Now });
            db.SubmitChanges();
            Console.WriteLine("db.Orders.Add(new Order { ProductID = 2, CustomerID = 2, OrderDate = DateTime.Now });");
            db.Orders.Add(new Order { EmployeeID = 1, CustomerID = "ALFKI", OrderDate = DateTime.Now });
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
#endif

            Console.Clear();
            Console.WriteLine("db.Orders.Remove(db.Orders.First());");
            db.Orders.DeleteOnSubmit(db.Orders.First());
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();

        }
    }
}
