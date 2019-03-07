#region MIT license
// 
// MIT license
//
// Copyright (c) 2009 Novell, Inc.
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
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.IO;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using NUnit.Framework;

using DbLinq.Null;

namespace DbLinqTest {

    class DummyConnection : IDbConnection
    {
        public DummyConnection()
        {
            ConnectionString = "";
        }

        public IDbTransaction BeginTransaction() {return null;}
        public IDbTransaction BeginTransaction(IsolationLevel il) {return null;}
        public void ChangeDatabase(string databaseName) {}
        public void Close() {}
        public IDbCommand CreateCommand() {return null;}
        public string ConnectionString{get; set;}
        public int ConnectionTimeout{get {return 0;}}
        public string Database{get {return null;}}
        public void Dispose() {}
        public void Open() {}
        public ConnectionState State{get {return ConnectionState.Closed;}}
    }

    [TestFixture]
    public class DataContextTest
    {
        DataContext context;

        [SetUp]
        public void SetUp()
        {
            context = new DataContext(new NullConnection() { ConnectionString = "" });
        }

        [TearDown]
        public void TearDown()
        {
            context = null;
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ConnectionStringNull()
        {
            string connectionString = null;
            new DataContext(connectionString);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ConnectionNull()
        {
            IDbConnection connection = null;
            new DataContext(connection);
        }

        [Test, ExpectedException(typeof(NullReferenceException))]
        public void Ctor_ConnectionStringOfConnectionIsNull()
        {
            IDbConnection connection = new NullConnection() { ConnectionString = null };
            new DataContext(connection);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Ctor_ConnectionString_DbLinqConnectionType_Empty()
        {
            new DataContext("DbLinqConnectionType=");
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Ctor_ConnectionString_DbLinqConnectionType_Empty2()
        {
            new DataContext("DbLinqConnectionType=;");
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Ctor_ConnectionString_DbLinqConnectionType_Invalid()
        {
            new DataContext("DbLinqConnectionType=InvalidType, DoesNotExist");
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Ctor_ConnectionString_DbLinqProvider_InvalidVendor()
        {
            new DataContext("DbLinqProvider=ThisVendorDoesNotExist");
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void Ctor_ConnectionString_DbLinqProvider_InvalidVendorWithDots()
        {
            new DataContext("DbLinqProvider=DbLinq.Sqlite.dll");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_FileOrServerOrConnectionIsNull()
        {
            MappingSource mapping = new AttributeMappingSource();
            string fileOrServerOrConnection = null;
            new DataContext(fileOrServerOrConnection, mapping);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_MappingIsNull()
        {
            MappingSource mapping = null;
            string fileOrServerOrConnection = null;
            new DataContext("", mapping);
        }

#if L2SQL
        // DbLinqProvider/etc. obviously aren't removed under L2SQL
        [ExpectedException(typeof(ArgumentException))]
#endif
        [Test]
        public void Ctor_ConnectionString_ExtraParameters_Munging()
        {
            if (Type.GetType("Mono.Runtime", false) != null)
                Assert.Ignore("Mono's System.Data.Linq is expected to remove DbLinq parameters.");
            DataContext ctx = new DataContext("Server=localhost;User id=test;Database=test;DbLinqProvider=Sqlite;DbLinqConnectionType=Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite");
            Assert.AreEqual(-1, ctx.Connection.ConnectionString.IndexOf("DbLinqProvider"));
            Assert.AreEqual(-1, ctx.Connection.ConnectionString.IndexOf("DbLinqConnectionType"));
        }
        
#if !L2SQL
        [Test, ExpectedException(typeof(NotImplementedException))]
        public void Ctor_FileOrServerOrConnectionIsFilename()
        {
            MappingSource mapping = new AttributeMappingSource();
            string fileOrServerOrConnection = typeof(DataContextTest).Assembly.Location;
            new DataContext(fileOrServerOrConnection, mapping);
        }

        [Test, ExpectedException(typeof(NotImplementedException))]
        public void Ctor_FileOrServerOrConnectionIsServer()
        {
            MappingSource mapping = new AttributeMappingSource();
            string fileOrServerOrConnection = "ThisIsAssumedToBeAServerName";
            new DataContext(fileOrServerOrConnection, mapping);
        }
#endif

        [Test]
        public void Connection()
        {
            IDbConnection connection = new NullConnection() { ConnectionString = "" };
            DataContext dc = new DataContext(connection);
            Assert.AreEqual(connection, dc.Connection);

#if !L2SQL
            dc = new DataContext (new DummyConnection());
            Assert.AreEqual(null, dc.Connection);
#endif
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteQuery_ElementTypeNull()
        {
            Type elementType = null;
            context.ExecuteQuery(elementType, "command");
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteQuery_QueryNull()
        {
            Type elementType = typeof(Person);
            context.ExecuteQuery(elementType, null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteQueryTResult_QueryNull()
        {
            context.ExecuteQuery<Person>(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetCommand_QueryNull()
        {
            IQueryable query = null;
            context.GetCommand(query);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetTable_TypeNull()
        {
            context.GetTable(null);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void GetTable_NotSupportedType()
        {
            context.GetTable(typeof(object));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void GetTableTEntity_NotSupportedType()
        {
            context.GetTable<object>();
        }

        [Test]
        public void GetTableTEntity()
        {
            Table<Person> table = context.GetTable<Person>();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Translate_ReaderNull()
        {
            context.Translate(typeof(Person), null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Translate_ElementTypeNull()
        {
            DbDataReader reader = new NullDataReader();
            context.Translate(null, reader);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void TranslateTResult_ReaderNull()
        {
            context.Translate<Person>(null);
        }
    }
}

