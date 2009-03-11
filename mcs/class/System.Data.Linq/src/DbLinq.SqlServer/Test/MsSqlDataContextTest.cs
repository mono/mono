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
using System.Linq;
using System.Reflection;

using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Mapping;

using DbLinq.Null;
using NUnit.Framework;

namespace DbLinqTest {

    [TestFixture]
    public class MsSqlDataContextTest : DataContextTestBase
    {
        static MsSqlDataContextTest()
        {
            // Make sure this assembly has a ref to DbLinq.SqlServer.dll.
            var dummy = new DbLinq.SqlServer.SqlServerSqlProvider();
        }

        protected override DataContext CreateDataContext()
        {
            return new DataContext (new NullConnection (), new AttributeMappingSource ());
        }

        protected override string People(string firstName)
        {
            return
                "SELECT [first_name], [last_name]\n" + 
                "FROM [people]\n" +
                "WHERE [first_name] = '" + firstName + "'";
        }

        protected override string People(string firstName, string lastName)
        {
            return People(firstName) + " AND [last_name] = '" + lastName + "'";
        }
    }
}

