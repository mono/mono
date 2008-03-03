//
// SqlClientMetaDataCollectionNamesTest.cs - NUnit Test Cases for testing the
//                          SqlClientMetaDataCollectionNames class
// Author:
//      Ankit Jain <jankit@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Data.SqlClient;
using NUnit.Framework;

#if NET_2_0
namespace MonoTests.System.Data.SqlClient
{
        [TestFixture]
        public class SqlClientMetaDataCollectionNamesTest
        {
                [Test]
                public void ValuesTest ()
                {
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Columns, "Columns");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Databases, "Databases");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.ForeignKeys, "ForeignKeys");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.IndexColumns, "IndexColumns");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Indexes, "Indexes");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Parameters, "Parameters");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.ProcedureColumns, "ProcedureColumns");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Procedures, "Procedures");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Tables, "Tables");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.UserDefinedTypes, "UserDefinedTypes");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Users, "Users");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.ViewColumns, "ViewColumns");
                        Assert.AreEqual (SqlClientMetaDataCollectionNames.Views, "Views");
                }
        }
}
#endif
