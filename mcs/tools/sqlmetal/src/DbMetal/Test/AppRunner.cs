#region MIT license
// 
// MIT license
//
// Copyright (c) 2010 Novell, Inc.
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
using System.IO;
using System.Linq;
using DbMetal;
using NUnit.Framework;

namespace DbMetal_Test_Sqlite
{
    class DbMetalAppDomainSetup : MarshalByRefObject
    {
        public void SetStandardError(TextWriter stderr)
        {
            Console.SetError(stderr);
        }

        public void Run(string[] args)
        {
            Program.Main(args);
        }
    }

    static class AppRunner
    {
#if MONO_STRICT
        const string Program                = "sqlmetal";
        const string DbConnectionProvider   = "Mono.Data.Sqlite.SqliteConnection, Mono.Data.Sqlite";
        const string DbLinqSchemaLoader     = "DbLinq.Vendor.DbSchemaLoader, System.Data.Linq";
        const string SqlDialect             = "DbLinq.Sqlite.SqliteVendor, System.Data.Linq";
#else
        const string Program                = "DbMetal";
        const string DbConnectionProvider   = "System.Data.SQLite.SQLiteConnection, System.Data.SQLite";
        const string DbLinqSchemaLoader     = "DbLinq.Vendor.DbSchemaLoader, DbLinq";
        const string SqlDialect             = "DbLinq.Sqlite.SqliteVendor, DbLinq.Sqlite";
#endif

        public static void WithinAppDomain(string expectedFile, string createdFile, IEnumerable<string> args)
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var info = new AppDomainSetup()
            {
                ApplicationBase     = bd,
                ApplicationName     = Program + ".exe",
                ConfigurationFile   = Program + ".exe.config",
            };
            AppDomain ad = AppDomain.CreateDomain("DbMetal Sqlite Test", null, info);
            var t = typeof(DbMetalAppDomainSetup);
            var s = (DbMetalAppDomainSetup)ad.CreateInstanceAndUnwrap(t.Assembly.GetName().Name, t.FullName);
            var stderr = new StringWriter();
            s.SetStandardError(stderr);
            var testdir = Path.Combine(bd, Path.Combine("..", "tests"));
            var expectedDir = Path.Combine(testdir, "expected");
            s.Run(new []{
                "/provider:Sqlite",
                "/conn:Data Source=" + Path.Combine(testdir, "Northwind.db3"),
            }.Concat(args).ToArray());
            AppDomain.Unload(ad);
            if (stderr.GetStringBuilder().Length != 0)
                Console.Error.Write(stderr.GetStringBuilder().ToString());
            Assert.AreEqual(0, stderr.GetStringBuilder().Length);
            FileAssert.AreEqual(Path.Combine(expectedDir, string.Format (expectedFile, Program)), createdFile);
            File.Delete(createdFile);
        }

        public static void WithDbSchemaLoader(string expectedFile, string createdFile, IEnumerable<string> args)
        {
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var testdir = Path.Combine(bd, Path.Combine("..", "tests"));
            var expectedDir = Path.Combine(testdir, "expected");

            DbMetal.Program.Main(new []{
                "/conn:Data Source=" + Path.Combine(testdir, "Northwind.db3"),
                "--with-dbconnection=" + DbConnectionProvider,
                "--with-schema-loader=" + DbLinqSchemaLoader,
                "--with-sql-dialect=" + SqlDialect,
            }.Concat(args).ToArray());

            FileAssert.AreEqual(Path.Combine(expectedDir, string.Format(expectedFile, Program)), createdFile);
            File.Delete(createdFile);
        }
    }
}