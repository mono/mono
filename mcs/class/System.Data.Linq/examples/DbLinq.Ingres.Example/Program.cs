#region MIT license
////////////////////////////////////////////////////////////////////
// MIT license:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Jiri George Moudry
//        Thomas Glaser
////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ingres.Client;

namespace DbLinq.Ingres.Example
{

    class Program
    {
        static void Main(string[] args)
        {
            IngresConnectionStringBuilder icsb = new IngresConnectionStringBuilder();
            icsb.Server = "(server)";
            icsb.Port = "II7";
            icsb.UserID = "LinqUser";
            icsb.Password = "linq2";
            icsb.Database = "northwind";
            nwind.Northwind db = new nwind.Northwind(new IngresConnection(icsb.ConnectionString));

            var result = from customer in db.Customers
                         where customer.City == "London"
                         orderby customer.City
                         select customer;

            foreach (var r in result)
            {
                r.Fax = "changed " + DateTime.Now.Ticks.ToString();
                System.Console.WriteLine(r);
            }

            db.SubmitChanges();

            System.Console.ReadKey();
        }
    }
}
