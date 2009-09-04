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
using System.Data;
using System.Linq;
using DbLinq.Factory;
using MySql.Data.MySqlClient;

using nwind;  // contains Northwind context

namespace DbLinq.MySql.Example
{
    class Program
    {
        static void Main(string[] args)
        {
/*            if (args.Length != 4)
            {
                Logger.Write("Usage: DbLinq.MySql.Example.exe server user password database");
                Logger.Write("Debug arguments can be set on project properties in visual studio.");
                Logger.Write("Press enter to continue.");
                Console.ReadLine();
                return;
            }

            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}", args);
            insertTest(connStr);
            return;
*/
#if false
            MySqlCommand cmd = new MySqlCommand("select hello(?s)", new MySqlConnection(connStr));
            //cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("?s", "xx");
            cmd.Parameters[0].Direction = ParameterDirection.Input; //.Value = "xx";
            cmd.Connection.Open();
            //MySqlDataReader dr = cmd.ExecuteReader();
            object obj = cmd.ExecuteScalar();
#endif
            string dbServer = Environment.GetEnvironmentVariable("DbLinqServer") ?? "localhost";
            // BUG: contexts must to be disposable
            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}"
                , dbServer, "LinqUser", "linq2", "Northwind");

            Northwind db = new Northwind(new MySqlConnection(connStr));

#if USE_STORED_PROCS
            int is2;
            object xx = db.sp_selOrders("ZZ", out is2);
            string reply0 = db.Hello0();
            string reply1 = db.Hello1("Pigafetta");
#endif

#if USE_AllTypes
            //Console.Clear();
            //Logger.Write("from at in db.Alltypes select at;");
            //var q1 = from at in db.Alltypes select at;
            //foreach (var v in q1)
            //    ObjectDumper.Write(v);
            //Logger.Write("Press enter to continue.");
            //Console.ReadLine();
#endif

            Console.Clear();
            Console.WriteLine( "from p in db.Products orderby p.ProductName select p;");
            var q2 = from p in db.Products orderby p.ProductName select p;
            foreach (var v in q2)
                ObjectDumper.Write(v);
            Console.WriteLine( "Press enter to continue.");
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
            Console.WriteLine( "from p in db.Products where p.ProductID == 7 select p;");
            var q4 = from p in db.Products where p.ProductID == 7 select p;
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine( "Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine( "from c in db.Customers from o in c.Orders where c.City == \"London\" select new { c, o };");
            var q5 = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine( "Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine( "from o in db.Orders where o.Customer.City == \"London\" select new { c = o.Customer, o };");
            var q6 = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine( "Press enter to continue.");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine( "db.Orders");
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine( "Press enter to continue.");
            Console.ReadLine();

            // BUG: This currently will insert 3 rows when it should insert only 2
            // SubmitChanges isn't clearing the client side transaction data
            //Console.Clear();
            //Console.WriteLine( "db.Orders.Add(new Order { ProductID = 7, CustomerId = 1, OrderDate = DateTime.Now });");
            //db.Orders.InsertOnSubmit(new Order { EmployeeID = 1, CustomerId = "ALFKI", OrderDate = DateTime.Now });
            //db.SubmitChanges();
            //Console.WriteLine( "db.Orders.Add(new Order { ProductID = 2, CustomerId = 2, OrderDate = DateTime.Now });");
            //db.Orders.InsertOnSubmit(new Order { EmployeeID = 1, CustomerId = "ALFKI", OrderDate = DateTime.Now });
            //db.SubmitChanges();
            //foreach (var v in db.Orders)
            //    ObjectDumper.Write(v);
            //Console.WriteLine("Press enter to continue.");
            //Console.ReadLine();

            Console.Clear();
            Console.WriteLine( "db.Orders.Remove(db.Orders.First());");
            db.Orders.DeleteOnSubmit(db.Orders.First());
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine( "Press enter to continue.");
            Console.ReadLine();

        }

        public static void insertTest(string connStr)
        {
            string sql = "insert into t3 (f1) VALUES (11); select @@IDENTITY";
            MySqlConnection conn = new MySqlConnection(connStr);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //int result = cmd.ExecuteNonQuery();
            object result = cmd.ExecuteScalar();
            Type tt = result.GetType();
            //result++;
        }

#if USE_AllTypes

        static void Main2(string[] args)
        {
            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}"
                , "localhost", "LinqUser", "linq2", "AllTypes");
            AllTypesExample.AllTypes db = new AllTypesExample.AllTypes(connStr);
            

            var list = db.Allinttypes.Where(at=>at.int_>0).ToList();
            var ct = list.Count;
        }
#endif
    }
}