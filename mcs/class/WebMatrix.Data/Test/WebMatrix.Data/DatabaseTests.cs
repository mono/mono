// 
// DatabaseTests.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Novell
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

#if NET_4_0

using System;
using System.IO;
using System.Linq;
using System.Data.Common;
using System.Collections.Generic;

using WebMatrix.Data;

using NUnit.Framework;

namespace MonoTests.WebMatrix.Data
{
	[TestFixtureAttribute]
	public class DatabaseTests
	{
		Database database;

		[SetUp]
		public void Setup ()
		{
			string path = Path.Combine ("Test", "testsqlite.db");
			database = Database.OpenConnectionString ("Data Source="+path+";Version=3;", "Mono.Data.Sqlite");
		}

		[Test]
		public void QuerySingleTest ()
		{
			var result = database.QuerySingle ("select * from memos where Text=@0 limit 1", "Grendel");

			Assert.IsNotNull (result);
			Assert.AreEqual ("Grendel", result.Text);
			Assert.AreEqual (5, result.Priority);
		}

		[Test]
		public void SimpleQueryTest ()
		{
			var result = database.Query ("select * from memos");

			Assert.IsNotNull (result);
			Assert.AreEqual (5, result.Count ());

			var col1 = new string[] { "Webmatrix", "Grendel", "Garuma", "jpobst", "Gonzalo" };
			var col2 = new object[] { 10, 5, -1, 6, 4 };
			int index = 0;

			foreach (var row in result) {
				Assert.AreEqual (col1[index], row.Text);
				Assert.AreEqual (col2[index], row.Priority);
				index++;
			}
		}

		[Test]
		public void ConnectionOpenedTest ()
		{
			bool opened = false;
			Database.ConnectionOpened += (sender, e) => opened = sender == database;

			var result = database.QuerySingle ("select * from memos where Text=@0 limit 1", "Grendel");

			Assert.IsTrue (opened);
		}
	}
}
#endif
