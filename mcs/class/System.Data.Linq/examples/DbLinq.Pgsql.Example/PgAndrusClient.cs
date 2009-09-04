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
using AndrusDB;

namespace ClientCodePg
{
    public class XReader { public int read() { return 11; } }

    public class PgAndrusClient
    {
        static void Main(string[] args)
        {
            Expression<Func<XReader, Employee>> newExpr = r => r.read() == 0
                ? new HourlyEmployee() { EmployeeID = r.read() } as Employee
                : new SalariedEmployee() { EmployeeID = r.read() } as Employee;


            Console.WriteLine("newExpr=" + newExpr);

            string connStr = "server=localhost;user id=LinqUser; password=linq2; database=andrus";

            using (Andrus db = new Andrus(connStr))
            {
                
                foreach (Employee emp in db.Employees)
                    Console.WriteLine(emp.Employeename);
            }

            Console.ReadLine();
            //Andrus db = new Andrus(connStr);
            //
            //Char_Pk charpk = db.Char_Pks.Single(c => c.Col1 == "a");
            //charpk.Val1 = 22;
            //db.SubmitChanges();
        }

    }
}
