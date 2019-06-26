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
using System.Collections.Generic;
using System.Linq;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using NUnit.Framework;

using DbLinq.Null;

namespace DbLinqTest
{
    [TestFixture]
    public class TableTest
    {
        Table<Person> people;

        [SetUp]
        public void SetUp()
        {
            people = new DataContext(new NullConnection() { ConnectionString = "" })
                .GetTable<Person>();
        }

        [TearDown]
        public void TearDown()
        {
            people = null;
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void Attach_EntityNull()
        {
            people.Attach(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AttachAll_EntitiesNull()
        {
            IEnumerable<Person> entities = null;
            people.AttachAll(entities);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void DeleteAllOnSubmit_EntitiesNull()
        {
            IEnumerable<Person> entities = null;
            people.DeleteAllOnSubmit(entities);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void DeleteOnSubmit_EntityNull()
        {
            people.DeleteOnSubmit(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetModifiedMembers_EntityNull()
        {
            people.GetModifiedMembers(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void GetOriginalEntityState_EntityNull()
        {
            people.GetOriginalEntityState(null);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void InsertAllOnSubmit_EntitiesNull()
        {
            IEnumerable<Person> entities = null;
            people.InsertAllOnSubmit(entities);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void InsertOnSubmit_EntityNull()
        {
            people.InsertOnSubmit(null);
        }

        [Test]
        public new void ToString()
        {
            Assert.AreEqual("Table(Person)", people.ToString());
        }
    }
}
