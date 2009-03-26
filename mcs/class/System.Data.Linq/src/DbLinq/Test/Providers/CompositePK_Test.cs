#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne
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
using NUnit.Framework;
using Test_NUnit;
using System.Data.Linq;

using nwind;

#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP
#else
        namespace Test_NUnit_Oracle
#endif
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL
#if MONO_STRICT
namespace Test_NUnit_MsSql_Strict
#else
    namespace Test_NUnit_MsSql
#endif
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#else
    #error unknown target
#endif
    {
    [TestFixture]
    public class CompositePK_Test : TestBase
    {
        protected void cleanup(Northwind db)
        {
            try
            {
                // Get the name of the Order Details table properly evaluating the Annotation
                string tableName = null;// db.Vendor.GetSqlFieldSafeName("order details"); //eg. "[Order Details]"
                foreach (object obj in typeof(OrderDetail).GetCustomAttributes(true))
                {
                    if (obj is System.Data.Linq.Mapping.TableAttribute)
                    {
                        tableName = ((System.Data.Linq.Mapping.TableAttribute)obj).Name;
                    }
                }
                string sql = string.Format("DELETE FROM {0} WHERE OrderID=3 AND ProductID=2", tableName);
                db.ExecuteCommand(sql);
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void CP1_DeletePreviousRows()
        {
            //delete any rows from previous testing
            Northwind db = CreateDB();
            // PC: this test was wrong, DeleteOnSubmit requires the object to be attached 
            // (by query result or manually, we chose here the query result)
            //var orderDetail = new OrderDetail { OrderID = 3, ProductID = 2 };
            //db.OrderDetails.DeleteOnSubmit(orderDetail);
            var toDelete = from o in db.OrderDetails where o.OrderID == 3 && o.ProductID == 2 select o;
            db.OrderDetails.DeleteAllOnSubmit(toDelete);
            db.SubmitChanges();
        }

        [Test]
        public void CP2_UpdateTableWithCompositePK()
        {
            Northwind db = CreateDB();
            cleanup(db);

            var orderDetail = new OrderDetail
            {
                OrderID = 3,
                ProductID = 2,
                UnitPrice = 20
            };

            db.OrderDetails.InsertOnSubmit(orderDetail);
            db.SubmitChanges();

            orderDetail.UnitPrice = 40;
            db.SubmitChanges();

            OrderDetail orderDetail2 = (from c in db.OrderDetails
                                        where c.UnitPrice == 40
                                        select c).Single();

            Assert.IsTrue(object.ReferenceEquals(orderDetail, orderDetail2), "Must be same object");

            Assert.AreEqual(3, orderDetail2.OrderID);
            Assert.AreEqual(2, orderDetail2.ProductID);
            Assert.AreEqual(40, orderDetail2.UnitPrice);

            db.OrderDetails.DeleteOnSubmit(orderDetail);
            db.SubmitChanges();
        }


        [Test]
        public void CP3_DeleteTableWithCompositePK()
        {
            Northwind db = CreateDB();
            cleanup(db);
            int initialCount = db.OrderDetails.Count();

            var orderDetail = new OrderDetail { OrderID = 2, ProductID = 2 };
            db.OrderDetails.InsertOnSubmit(orderDetail);
            db.SubmitChanges();

            Assert.AreEqual(db.OrderDetails.Count(), initialCount + 1);
            db.OrderDetails.DeleteOnSubmit(orderDetail);
            db.SubmitChanges();

            Assert.AreEqual(db.OrderDetails.Count(), initialCount);
        }

        [Test]
        [ExpectedException(typeof(ChangeConflictException))]
        public void CP4_UnchangedColumnShouldNotUpdated()
        {
            Random rand = new Random();

            Northwind db = CreateDB();
            var orderDetail = new OrderDetail { OrderID = 1, ProductID=2};
            db.OrderDetails.Attach(orderDetail);

            float newDiscount = 15 + (float)rand.NextDouble();
            orderDetail.Discount = newDiscount;
            db.SubmitChanges();

            //this test is bad conceptually, for this reason last two lines has been commented and now a changeConflictException is expected.
            //This is the behaviour in linq2sl.

            //var orderDetail2 = db.OrderDetails.Single(od => od.OrderID == 1);
            //Assert.AreEqual((float)orderDetail2.Discount, newDiscount);
        }

        [Test(Description = "Check that both keys are used to determine identity")]
        public void CP5_Composite_ObjectIdentity()
        {
            Northwind db = CreateDB();
            var q = db.OrderDetails.Where(od => od.ProductID == 2 && od.OrderID == 1);
            OrderDetail row1 = q.Single();
            OrderDetail row2 = q.Single();
            Assert.IsTrue(object.ReferenceEquals(row1, row2));
        }


    }
}
