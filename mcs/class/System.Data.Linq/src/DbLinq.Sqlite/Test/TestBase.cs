#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.Data.SQLite;
using System.IO;

namespace Test_NUnit
{
    public abstract partial class TestBase
    {
        static bool doRecreate = true;
        public const StringComparison stringComparisonType = StringComparison.InvariantCulture;

        static partial void CheckRecreateSqlite ()
        {
            if (doRecreate) {
                File.Copy ("../src/Northwind.db3", "Northwind.db3", true);
                doRecreate = false;
            }
        }

        public static IDbCommand CreateCommand (string sql, IDbConnection conn)
        {
            return new SQLiteCommand(sql, (SQLiteConnection) conn);
        }

        public static IDbConnection CreateConnection (string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        public DbLinq.Vendor.IVendor CreateVendor()
        {
            return new DbLinq.Sqlite.SqliteVendor();
        }
    }
}
