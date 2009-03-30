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
using System.Data;
using System.IO;
using System.Xml;
using System.Reflection;
using NUnit.Framework;

using nwind;

namespace Test_NUnit
{
    /// <summary>
    /// base class for ReadTest and WriteTest. 
    /// Provides CreateDB(), Conn, and stringComparisonType.
    /// </summary>
    public abstract partial class TestBase
    {
        public string DbServer
        {
            get
            {
                return Environment.GetEnvironmentVariable("DbLinqServer") ?? "localhost";
            }
        }
        public string connStr
        {
            get
            {
                var xConnectionStringsDoc = new XmlDocument();
                xConnectionStringsDoc.Load("../src/ConnectionStrings.xml");
                XmlNode currentAssemblyNode = xConnectionStringsDoc.SelectSingleNode(string.Format("//Connection[@assembly=\"{0}\"]", Assembly.GetCallingAssembly().GetName().Name));
                string stringConnection = currentAssemblyNode.FirstChild.Value.Replace(@"\\", @"\");
								Console.WriteLine ("# Assembly={0}; connectionString={1}",
										Assembly.GetCallingAssembly().GetName().Name,
										stringConnection);
                if (stringConnection.Contains("{0}"))
                    stringConnection = string.Format(stringConnection, DbServer);
                return stringConnection;
            }
        }
        IDbConnection _conn;
        public IDbConnection Conn
        {
            get
            {
                if (_conn == null) { _conn = CreateConnection(connStr); _conn.Open(); }
                return _conn;
            }
        }

        //public Northwind CreateDB()
        //{
        //    return CreateDB(System.Data.ConnectionState.Closed);
        //}

        static partial void CheckRecreateSqlite ();

        public Northwind CreateDB()
        {
            return CreateDB(System.Data.ConnectionState.Closed);
        }

        public Northwind CreateDB(System.Data.ConnectionState state)
        {
            CheckRecreateSqlite();
            var conn = CreateConnection(connStr);
            if (state == System.Data.ConnectionState.Open)
                conn.Open();
            var db = new Northwind(conn) { Log = Console.Out };
            return db;
        }

        /// <summary>
        /// execute a sql statement, return an Int64.
        /// </summary>
        public long ExecuteScalar(string sql)
        {
            using (var cmd = CreateCommand(sql, Conn))
            {
                object oResult = cmd.ExecuteScalar();
                Assert.IsNotNull("Expecting result, instead got null. (sql=" + sql + ")");
                Assert.IsInstanceOfType(typeof(long), oResult, "Expecting 'long' result from query " + sql + ", instead got type " + oResult.GetType());
                return (long)oResult;
            }
        }

        /// <summary>
        /// execute a sql statement
        /// </summary>
        public void ExecuteNonQuery(string sql)
        {
            using (var cmd = CreateCommand(sql, Conn))
            {
                int iResult = cmd.ExecuteNonQuery();
            }
        }

        public static Product NewProduct(string productName)
        {
            var p = new Product
            {
                ProductName = productName,
                SupplierID = 1,
                CategoryID = 1,
                QuantityPerUnit = "11",
#if ORACLE || FIREBIRD
                UnitPrice = 11, //type "int?"
#else
                UnitPrice = 11m,
#endif
                UnitsInStock = 23,
                UnitsOnOrder = 0,
            };
            return p;
        }
    }
}
