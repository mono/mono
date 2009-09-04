using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebirdSql.Data.FirebirdClient;
using System.Data.Linq;
using System.IO;
using nwind;

// DbMetal.exe /server:localhost /user:sysdba /password:masterkey "/database=C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE.FDB" /provider:FirebirdSql /code:..\..\..\examples\DbLinq.FbSql.Example\Employee.cs
// DbMetal.exe /server:localhost /user:sysdba /password:masterkey "/database=C:\Program Files\Firebird\Firebird_2_1\examples\nwind\NORTHWIND.FDB" /provider:FirebirdSql /code:..\..\..\examples\DbLinq.FbSql.Example\Northwind.cs

namespace DbLinq.FbSql.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string databasePath = Path.Combine(programFiles,
                @"Firebird\Firebird_2_1\examples\nwind\NORTHWIND.FDB");
            string connStr = string.Format("Server={0};Database={1};User={2};Password={3}",
                "localhost", databasePath, "sysdba", "masterkey");

            Northwind db = new Northwind(new FbConnection(connStr));

            // BUG: Fixes problem deleting orders below.
            var x = db.OrderDetails.First();
            ObjectDumper.Write(x);

            Console.Clear();
            Console.WriteLine("from p in db.Products orderby p.ProductName select p;");
            var q2 = from p in db.Products orderby p.ProductName select p;
            foreach (var v in q2)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            Console.Clear();
            var q3 = from c in db.Customers
                     from o in c.Orders
                     where c.City == "London"
                     select new { c, o };
            foreach (var v in q3)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("from p in db.Products where p.ProductID == 7 select p;");
            var q4 = from p in db.Products where p.ProductID == 7 select p;
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("from c in db.Customers from o in c.Orders where c.City == \"London\" select new { c, o };");
            var q5 = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("from o in db.Orders where o.Customer.City == \"London\" select new { c = o.Customer, o };");
            var q6 = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };
            foreach (var v in q4)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("db.Orders");
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            // BUG: auto_increment columns aren't supported on Firebird SQL yet.
            Console.Clear();
            Console.WriteLine("db.Orders.Add(new Order { ProductID = 7, CustomerId = 1, OrderDate = DateTime.Now });");
            db.Orders.InsertOnSubmit(new Order { EmployeeID = 1, CustomerID = "ALFKI", OrderDate = DateTime.Now });
            db.SubmitChanges();
            Console.WriteLine("db.Orders.Add(new Order { ProductID = 2, CustomerId = 2, OrderDate = DateTime.Now });");
            db.Orders.InsertOnSubmit(new Order { EmployeeID = 1, CustomerID = "ALFKI", OrderDate = DateTime.Now });
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("db.Orders.Remove(db.Orders.First());");
            var order = db.Orders.First();
            db.Orders.DeleteOnSubmit(order);
            db.SubmitChanges();
            foreach (var v in db.Orders)
                ObjectDumper.Write(v);
            Console.WriteLine("Press enter to continue.");
            Console.ReadKey();
        }
    }
}
