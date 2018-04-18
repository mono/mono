//
// SqlParameterCollectionTest.cs - NUnit Test Cases for testing the
//                          SqlParameterCollection class
// Author:
//      Amit Biswas (amit@amitbiswas.com)
//
// Copyright (c) 2007 Novell Inc., and the individuals listed
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

namespace MonoTests.System.Data.Connected.SqlClient
{
	[TestFixture]
	[Category ("sqlserver")]

	public class SqlParameterCollectionTest
	{
		EngineConfig engine;

		[SetUp]
		public void SetUp ()
		{
			engine = ConnectionManager.Instance.Sql.EngineConfig;
		}

		[Test]
		public void CopyToTest ()
		{
			SqlCommand cmd = new SqlCommand ();
			cmd.CommandText = "SELECT fname FROM employee WHERE fname=@fname AND lname=@lname";
			SqlParameter p1Fname = cmd.Parameters.Add ("@fname", SqlDbType.VarChar, 15);
			SqlParameter p1Lname = cmd.Parameters.Add ("@lname", SqlDbType.VarChar, 15);

			Assert.AreEqual (2, cmd.Parameters.Count, "#1 Initialization error, parameter collection must contain 2 elements");

			SqlParameter [] destinationArray = new SqlParameter [4];
			cmd.Parameters.CopyTo (destinationArray, 1);
			Assert.AreEqual (4, destinationArray.Length, "#2 The length of destination array should not change");
			Assert.AreEqual (null, destinationArray[0], "#3 The parameter collection is copied at index 1, so the first element should not change");
			Assert.AreEqual (p1Fname, destinationArray[1], "#4 The parameter at index 1 must be p1Fname");
			Assert.AreEqual (p1Lname, destinationArray[2], "#5 The parameter at index 2 must be p1Lname");
			Assert.AreEqual (null, destinationArray[3], "#6 The parameter at index 3 must not change");
		}
	}
}
