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
using System.Data.SqlTypes;

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

		[Test]
		public void ParameterType ()
		{
			// If Type is not set, then type is inferred from the value
			// assigned. The Type should inferred everytime Value is assigned
			// If value is null/DBNull, then the current Type should retained.
			SqlParameter param = new SqlParameter ();
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#1");
			param.Value = DBNull.Value;
			Assert.AreEqual (SqlDbType.NVarChar, param.SqlDbType, "#2");
			param.Value = 1;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#3");
			param.Value = DBNull.Value;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#4");
			param.Value = null;
			Assert.AreEqual (SqlDbType.Int, param.SqlDbType, "#4");

			//If Type is set, then the Type should not inferred from the value 
			//assigned.
			SqlParameter param1 = new SqlParameter ();
			param1.DbType = DbType.String; 
			Assert.AreEqual (SqlDbType.NVarChar, param1.SqlDbType, "#5");
			param1.Value = 1;
			Assert.AreEqual (SqlDbType.NVarChar, param1.SqlDbType, "#6");

			SqlParameter param2 = new SqlParameter ();
			param2.SqlDbType = SqlDbType.NVarChar;
			Assert.AreEqual (SqlDbType.NVarChar, param2.SqlDbType, "#7");
			param2.Value = 1;
			Assert.AreEqual (SqlDbType.NVarChar, param2.SqlDbType, "#8");
		}

#if NET_2_0
		[Test]
		public void CompareInfoTest ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (SqlCompareOptions.None, parameter.CompareInfo, "#1 Default value should be System.Data.SqlTypes.SqlCompareOptions.None");

			parameter.CompareInfo = SqlCompareOptions.IgnoreNonSpace;
			Assert.AreEqual (SqlCompareOptions.IgnoreNonSpace, parameter.CompareInfo, "#2 It should return CompareOptions.IgnoreSpace after setting this value for the property");
		}
	
		[Test]
		public void LocaleIdTest ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (0, parameter.LocaleId, "#1 Default value for the property should be 0");

			parameter.LocaleId = 15;
			Assert.AreEqual(15, parameter.LocaleId, "#2");
		}

		[Test]
		public void SqlValue ()
		{
			SqlParameter parameter = new SqlParameter ();
			Assert.AreEqual (null, parameter.SqlValue, "#1 Default value for the property should be Null");

			parameter.SqlValue = SqlDbType.Char.ToString ();
			Assert.AreEqual ("Char", parameter.SqlValue, "#1 The value for the property should be Char after setting SqlDbType to Char");
		}
#endif

	}
}
