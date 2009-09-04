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
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using nwind;

namespace DbLinq.Mssql.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            string connStr = "Data Source=.\\SQLExpress;Integrated Security=True;Initial Catalog=Northwind";
             Northwind db = new Northwind(new SqlConnection(connStr));


            var res = from cust in db.Customers
                      select cust.CompanyName;


            foreach (var r in res.ToList())
                Console.WriteLine(r);

            
            //var regions = db.Regions.ToList();

            //Vendor.UseBulkInsert[db.Regions] = true;
            //db.Regions.Add(new Region(-1, "tmp_region1"));
            //db.Regions.Add(new Region(-2, "tmp_region2"));

            //DbLinq.vendor.mssql.VendorMssql.UseBulkInsert[db.Shippers] = true;
            //db.Shippers.Add(new Shippers(-1, "UPS", "800-800-8888"));
            //db.Shippers.Add(new Shippers(-1, "Fedex", "900-900-9999"));
            //db.SubmitChanges();

        }
    }
}
