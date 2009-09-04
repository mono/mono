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
using System.Linq;

#if DEBUG
//using System.Data.OracleClient;
//using XSqlCommand = System.Data.OracleClient.OracleCommand;
#endif

#if ORACLE_PROVIDER
//Oracle provider - download from http://www.oracle.com/technology/software/tech/windows/odpnet/utilsoft_11gbeta.html
using Oracle.DataAccess.Client;
#else
using System.Data.OracleClient;

#endif


namespace nwind
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = "server=localhost;user=Northwind;password=linq2";
            insertTest(connStr);
            return;

            //Northwind db = new Northwind(connStr);
            //var q = from at in db.alltypes select at;
            //var q = from p in db.products orderby p.ProductName select p;
            //var q = from c in db.customers from o in c.Orders 
            //        where c.City == "London" select new { c, o };

            int insertedID = 7;
            //var q = from p in db.Products where p.ProductID==insertedID select p;
            //var q = from p in db.Products //where p.ProductID==insertedID 
            //        select p;
            //int ii = q.Count();
            //var q = from c in db.Customers from o in c.Orders where c.City == "London" select new { c, o };
            //It’s also possible to do the reverse.
            //var q1 = from c in cc where c.CustomerID==0 select c.CustomerID.Select(0);
            //var q = from o in db.Orders where o.Customer.City == "London" select new { c = o.Customer, o };


            //string queryText = db.GetQueryText(q);
            //Console.WriteLine("User sees sql:"+queryText);


            //foreach (var v in q)
            //{
            //    Console.WriteLine("OBJ:" + v);
            //}
        }

        public static void insertTest(string connStr)
        {
            //            string sql = @"
            //
            //select t1_seq.CurrVal FROM DUAL
            //";
            //string sql = @"BEGIN SELECT 12 INTO :1 FROM DUAL; END;";
            string sql = "BEGIN insert INTO t1 (id1) values (t1_seq.NextVal);\n select t1_seq.CurrVal INTO :1 FROM DUAL; END;";
#if ORACLE_PROVIDER
            //Oracle provider - download from http://www.oracle.com/technology/software/tech/windows/odpnet/utilsoft_11gbeta.html
            //Oracle provider (check via TNSPING): "User Id=Scott;Password=tiger;Data Source=orcl9i"
            connStr = "User Id=Northwind;Password=linq2;Data Source=XE";
#else
            //Microsoft provider: System.Data.OracleClient
            //see http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=1055859&SiteID=1
#endif
            OracleConnection conn = new OracleConnection(connStr);
            conn.Open();

            //OracleCommand cmd = new OracleCommand(sql, conn);
            //cmd.CommandType = System.Data.CommandType.Text;
            OracleCommand cmd = conn.CreateCommand();
            OracleParameter p1 = new OracleParameter("1", OracleType.Number);
            p1.Direction = System.Data.ParameterDirection.Output;
            cmd.Parameters.Add(p1);

            OracleString os = new OracleString();
            try
            {
                //conn.Open();
                try
                {
                    //cmd.CommandText = "begin dbms_output.enable; end;";
                    cmd.CommandText = sql;
                    //cmd.ExecuteOracleNonQuery(out os);
                    object obj1 = cmd.ExecuteScalar();
                    Type tt2 = obj1.GetType();
                }
                catch (OracleException ex)
                {
                    //MessageBox.Show(ex.Message, "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("Failed: " + ex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed: " + ex);
            }
            //int result = cmd.ExecuteNonQuery();
            //object result = cmd.ExecuteOracleScalar();
            object result = cmd.ExecuteScalar();
            Type tt = result.GetType();
            //result++;
        }

        //static void Main()
        //{
        //    //OracleDataReader returning no rows or data even though data exists
        //    //www.devnewsgroups.net/group/microsoft.public.dotnet.framework.adonet/topic3406.aspx

        //    string sql = @"SELECT ProductID FROM products WHERE ProductName=:p1"; //returns no rows
        //    //string sql = @"SELECT * FROM USER_ALL_TABLES"; //returns many rows

        //    OracleConnection conn = new OracleConnection("server=localhost;user=system;password=linq2");
        //    conn.Open();
        //    OracleCommand cmd = new OracleCommand(sql,conn);
        //    cmd.Parameters.Add(":p1", "Pen");
        //    OracleDataReader rdr = cmd.ExecuteReader(System.Data.CommandBehavior.Default);
        //    //OracleDataReader rdr = cmd.exe.exe();
        //    string x = rdr.FieldCount + " "+ rdr.HasRows;
        //}
    }
}
