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
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using nwind;
using Npgsql;

namespace ClientCode2
{

    class Program
    {

        static void Main_(string[] args)
        {

            string connStr = "server=localhost;user id=LinqUser; password=linq2; database=northwind";
            //NpgsqlConnection conn = new NpgsqlConnection(connStr);
            //conn.Open();
            ////string sql = "INSERT City (Name) VALUES ('B'); SELECT @@IDENTITY";
            //string sql = "INSERT City (Name) VALUES ('C1'), ('C2'); SELECT @@IDENTITY; SELECT @@IDENTITY";
            //NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
            ////object obj1 = cmd.ExecuteScalar();
            ////string s1 = obj1.ToString();
            //NpgsqlDataReader rdr = cmd.ExecuteReader();
            //int fields = rdr.FieldCount;
            //while(rdr.Read())
            //{
            //    object obj1 = rdr.GetValue(0);
            //    string s1 = obj1.ToString();
            //}

            //TestContext db = new TestContext(connStr);
            int[] arr = new int[] { 2, 3, 2, 5 };
            var q3 = from a in arr select a.ToString() + "i";
            var bb = q3.ToList();

            //arr.
            //var q7 = arr.Any(.Distinct().ToArray();

            //Northwind db = new Northwind(connStr);
            ////var q = from at in db.alltypes select at;
            ////var q = from p in db.products orderby p.ProductName select p;
            //var q = from c in db.Customers
            //        from o in c.Orders
            //        where c.City == "London"
            //        select new { c, o };

            //foreach (var v in q)
            //{
            //    Console.WriteLine("OBJ:" + v.c.ContactName);
            //}
            //db.SaveChanges();
        }

        void insertChildRecord()
        {
        }

        //Customer prototypeLiveObjectCache()
        //{
        //    Dictionary<int,Customer> liveObjects;
        //    Customer c1 = null;//fromDB();
        //    Customer c2;
        //    //if(liveObjects.TryGetValue(c1.CustomerID, out c2))
        //    //{
        //    //    return c2; //discard c1
        //    //}
        //    //liveObjects[c1.CustomerID] = c1;
        //    return c1;
        //}

    }
}
