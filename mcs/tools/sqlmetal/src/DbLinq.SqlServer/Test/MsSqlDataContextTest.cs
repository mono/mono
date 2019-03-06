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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq;
using System.Data.Linq.Mapping;
#else
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Mapping;
#endif

using DbLinq.Null;
using NUnit.Framework;

namespace DbLinqTest {

    [TestFixture]
    public class MsSqlDataContextTest : DataContextTestBase
    {
        static MsSqlDataContextTest()
        {
#if !MONO_STRICT
            // Make sure this assembly has a ref to DbLinq.SqlServer.dll.
            var dummy = new DbLinq.SqlServer.SqlServerSqlProvider();
#endif
        }

        protected override DataContext CreateDataContext()
        {
            return new DataContext (new NullConnection (), new AttributeMappingSource ());
        }

        protected override string People(string firstName)
        {
            return string.Format(
                "SELECT [first_name], [last_name]{0}" + 
                "FROM [people]{0}" +
                "WHERE ([first_name] = '" + firstName + "')", 
                Environment.NewLine); ;
        }

        protected override string People(string firstName, string lastName)
        {
            return People(firstName) + " AND ([last_name] = '" + lastName + "')";
        }

        protected override string People(string firstName, string lastName, int skip, int take)
        {
            return string.Format("SELECT *{0}" +
                "FROM ({0}" +
                "    SELECT [first_name], [last_name]{0}" +
                ",{0}" +
                "    ROW_NUMBER() OVER(ORDER BY [first_name], [last_name]{0}" +
                ") AS [__ROW_NUMBER]{0}" +
                "    FROM [people]{0}" +
                "WHERE ([first_name] = '{1}') AND ([last_name] = '{2}')    ) AS [t0]{0}" +
                "WHERE [__ROW_NUMBER] BETWEEN {3}+1 AND {3}+{4}{0}" +
                "ORDER BY [__ROW_NUMBER]",
                Environment.NewLine, firstName, lastName, skip, take);
        }

        [Test]
        public void Count()
        {
            var oldLog = Context.Log;
            var log = new StringWriter();
            try
            {
                Context.Log = log;
                (from p in Context.GetTable<Person>()
                     orderby p.LastName
                     select p)
                    .Count();
            }
            catch (NotSupportedException)
            {
                Console.WriteLine("# logfile=\n{0}", log.ToString());
                var expected = string.Format("SELECT COUNT(*){0}" +
                    "FROM [people]{0}" +
                    "--",
                    Environment.NewLine);
                Assert.IsTrue(log.ToString().Contains(expected));
            }
            catch (Exception e)
            {
                Assert.Fail("# ExecuteCommand: Got exception {0}", e.ToString());
            }
            finally
            {
                Context.Log = oldLog;
            }
        }
    }
}

