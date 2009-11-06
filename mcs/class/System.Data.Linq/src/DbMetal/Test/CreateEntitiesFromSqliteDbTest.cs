#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
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
using System.IO;
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

    [TestFixture]
    public class CreateEntitiesFromSqliteDbTest
    {
        [Test]
        public void Create()
        {
#if MONO_STRICT
            var app = "sqlmetal";
#else
            var app = "DbMetal";
#endif
            var bd = AppDomain.CurrentDomain.BaseDirectory;
            var info = new AppDomainSetup()
            {
                ApplicationBase     = bd,
                ApplicationName     = app + ".exe",
                ConfigurationFile   = app + ".exe.config",
            };
            AppDomain ad = AppDomain.CreateDomain("DbMetal Sqlite Test", null, info);
            var t = typeof(DbMetalAppDomainSetup);
            var s = (DbMetalAppDomainSetup)ad.CreateInstanceAndUnwrap(t.Assembly.GetName().Name, t.FullName);
            var stderr = new StringWriter();
            s.SetStandardError(stderr);
            var testdir = Path.Combine(bd, Path.Combine("..", "tests"));
            var db = Path.Combine(bd, Path.Combine("..", Path.Combine("tests", "Northwind.db3")));
            s.Run(new string[]{
                "/code:Northwind.Sqlite.cs",
                "/conn:Data Source=" + Path.Combine(testdir, "Northwind.db3"),
                "/database:Northwind",
                "--generate-timestamps-",
                "/namespace:nwind",
                "/pluralize",
                "/provider:Sqlite",
            });
            AppDomain.Unload(ad);
            if (stderr.GetStringBuilder().Length != 0)
                Console.Error.Write(stderr.GetStringBuilder().ToString());
            Assert.AreEqual(0, stderr.GetStringBuilder().Length);
            FileAssert.AreEqual(Path.Combine(testdir, "Northwind.Expected.Sqlite-" + app + ".cs"), "Northwind.Sqlite.cs");
            File.Delete("Northwind.Sqlite.cs");
        }
    }
}