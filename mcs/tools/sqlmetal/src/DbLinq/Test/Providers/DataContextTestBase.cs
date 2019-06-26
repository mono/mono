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
// using System.Data.Linq;
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

    public abstract class DataContextTestBase
    {
        DataContext context;

        protected DataContext Context {
            get { return context; }
        }

        [SetUp]
        public void SetUp()
        {
            context = CreateDataContext();
        }

        protected abstract DataContext CreateDataContext();

        [TearDown]
        public void TearDown()
        {
            context = null;
        }

        [Test]
        public void ExecuteCommand()
        {
            context.Log = new StringWriter ();
            try 
            {
                context.ExecuteCommand("SomeCommand", 1, 2, 3);
            }
            catch (NotSupportedException)
            {
            }
            catch (Exception e)
            {
                Assert.Fail("# ExecuteCommand: Got exception {0}", e.ToString());
            }
            Console.WriteLine ("# ExecuteCommand: Log={0}", context.Log);
        }

        [Test]
        public void ExecuteQuery()
        {
            context.Log = new StringWriter ();
            try 
            {
                context.ExecuteQuery(typeof(Person), "select * from people", 1, 2, 3);
            }
            catch (NotSupportedException)
            {
            }
            catch (Exception e)
            {
                Assert.Fail("# ExecuteQuery: unexpected exception: {0}", e.ToString());
            }
            Console.WriteLine ("# ExecuteQuery: Log={0}", context.Log);
        }

        [Test]
        public void ExecuteQueryTResult()
        {
            context.Log = new StringWriter ();
            try 
            {
                context.ExecuteQuery<Person>("select * from people", 1, 2, 3);
            }
            catch (NotSupportedException)
            {
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            Console.WriteLine ("# ExecuteQueryTResult: Log={0}", context.Log);
        }

        [Test]
        public void GetChangeSet()
        {
            // TODO
            context.GetChangeSet();
        }

        protected abstract string People(string firstName);
        protected abstract string People(string firstName, string lastName);
        protected abstract string People(string firstName, string lastName, int skip, int take);

        [Test]
        public void GetCommand()
        {
            var foos = 
                from p in context.GetTable<Person>()
                where p.FirstName == "foo"
                select p;
            var cmd = context.GetCommand(foos);

            Assert.AreEqual(People("foo"), cmd.CommandText);

            foos = foos.Where(p => p.LastName == "bar");
            var cmd2 = context.GetCommand(foos);

            Assert.IsFalse(object.ReferenceEquals(cmd, cmd2));

            Assert.AreEqual(People("foo", "bar"), cmd2.CommandText);

            foos = foos.Skip(1).Take(2);
            cmd = context.GetCommand(foos);
            Assert.AreEqual(People("foo", "bar", 1, 2), cmd.CommandText);
        }
    }
}

