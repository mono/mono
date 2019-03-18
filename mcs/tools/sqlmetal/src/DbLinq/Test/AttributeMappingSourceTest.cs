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
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

using System.Data.Linq.Mapping;

#if MONO_STRICT
using System.Data.Linq;
using AttributeMappingSource = System.Data.Linq.Mapping.AttributeMappingSource;
#else
using DbLinq.Data.Linq;
using AttributeMappingSource = DbLinq.Data.Linq.Mapping.AttributeMappingSource;
#endif

using DbLinq.Null;
using NUnit.Framework;

namespace DbLinqTest
{
    [Table(Name = "dbo...FooTable")]
    class Foo
    {
        [Column(Name="Col1")]
        public string Column1 { get; set; }
    }

    [Database(Name = "MyDB1")]
    class MyDataContext2 : DataContext
    {
        public MyDataContext2()
            : base(new SqlConnection("Data Source=localhost"))
        {
        }

        public Table<Foo> FooTable { get { return GetTable<Foo>(); } }
        public Table<Foo> FooFieldTable;
    }

    [TestFixture]
    public class AttributeMappingSourceTest
    {
        [Test]
        public void CreateModel_GetTables_Has_No_Duplicates()
        {
            var model = new AttributeMappingSource().GetModel(typeof(MyDataContext2));
            var tables = model.GetTables().ToList();
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual("dbo...FooTable", tables[0].TableName);
        }
    }
}
