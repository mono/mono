//
// SqlParameterTest.cs - NUnit Test Cases for testing the
//                          SqlParameter class
// Author:
//      Senganal T (tsenganal@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]
	//FIXME : Add more testcases
	public class SqlParameterTest
	{
		//testcase for #77410
		[Test]
		public void ParameterNullTest ()
		{
			SqlParameter param = new SqlParameter ("param", SqlDbType.Decimal);
			Assert.AreEqual (0, param.Scale, "#1");
			param.Value = DBNull.Value;
			Assert.AreEqual (0, param.Scale, "#2");

			param = new SqlParameter ("param", SqlDbType.Int);
			Assert.AreEqual (0, param.Scale, "#3");
			param.Value = DBNull.Value;
			Assert.AreEqual (0, param.Scale, "#4");
		}
	}
}
