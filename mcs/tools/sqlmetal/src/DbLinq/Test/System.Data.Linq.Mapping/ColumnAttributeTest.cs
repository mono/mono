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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

using System.Data.Linq.Mapping;

using DbLinq.Null;
using NUnit.Framework;

namespace System.Data.Linq.Mapping.Test
{
    [TestFixture]
    public class ColumnAttributeTest
    {
        [Test]
        public void Ctor()
        {
            var c = new ColumnAttribute();
            Assert.AreEqual(AutoSync.Default,   c.AutoSync);
            Assert.AreEqual(true,               c.CanBeNull);
            Assert.AreEqual(null,               c.DbType);
            Assert.AreEqual(null,               c.Expression);
            Assert.AreEqual(false,              c.IsDbGenerated);
            Assert.AreEqual(false,              c.IsDiscriminator);
            Assert.AreEqual(false,              c.IsVersion);
            Assert.AreEqual(UpdateCheck.Always, c.UpdateCheck);
            Assert.AreEqual(false,              c.IsPrimaryKey);
            Assert.AreEqual(null,               c.Name);
            Assert.AreEqual(null,               c.Storage);
            Assert.AreEqual(c.GetType(),        c.TypeId);
        }
    }
}
