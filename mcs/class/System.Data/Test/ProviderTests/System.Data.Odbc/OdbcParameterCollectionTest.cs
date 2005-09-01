//
// OdbcParameterCollectionTest.cs - NUnit Test Cases for testing the
//			  OdbcParameterCollection class
// Author:
//      Sureshkumar T (TSureshkumar@novell.com)
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
using System.Text;
using System.Data;
using System.Data.Odbc;

using NUnit.Framework;

namespace MonoTests.System.Data.Odbc
{
	[TestFixture]
	[Category ("odbc")]
	public class OdbcParameterCollectionTest
	{
		/// <remarks>
		/// This tests whether the value is trimmed to the
		/// given length while passing parameters
		/// </remarks>
		[Test]
		public void ParameterLengthTrimTest ()
		{
			IDbConnection conn = ConnectionManager.Singleton.Connection;
			try {
				ConnectionManager.Singleton.OpenConnection ();
				OdbcCommand cmd = (OdbcCommand) conn.CreateCommand ();
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = "SELECT count(*) FROM employee WHERE fname=?";
															     
				OdbcParameter param = cmd.Parameters.Add("@fname", OdbcType.Text, 15);
				param.Value = DateTime.Now.ToString ();
				Assert.AreEqual (15, param.Size, "#1");
				Convert.ToInt32(cmd.ExecuteScalar());
			} finally {
				ConnectionManager.Singleton.CloseConnection ();
			}
                }
        }
}
